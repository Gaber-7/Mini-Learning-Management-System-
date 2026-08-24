using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MiniLMS.Business.DTOs;
using MiniLMS.Business.Interfaces;
using MiniLMS.Data.Data;
using MiniLMS.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniLMS.Business.Services
{
    public class InstructorService : IInstructorService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public InstructorService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // ================= Profile =================

        public async Task<InstructorProfileDto?> GetProfileAsync(int instructorId)
        {
            var instructor = await _context.Instructors
                .Include(i => i.Courses)
                    .ThenInclude(c => c.Lessons)
                .FirstOrDefaultAsync(i => i.Id == instructorId);

            if (instructor == null) return null;

            var dto = _mapper.Map<InstructorProfileDto>(instructor);
            dto.TotalCourses = instructor.Courses.Count;

            var courseIds = instructor.Courses.Select(c => c.Id).ToList();
            dto.TotalStudents = await _context.Enrollments.CountAsync(e => courseIds.Contains(e.CourseId));

            return dto;
        }

        public async Task<bool> UpdateProfileAsync(int instructorId, UpdateInstructorProfileDto dto)
        {
            var instructor = await _context.Instructors.FindAsync(instructorId);
            if (instructor == null) return false;

            instructor.FullName = dto.FullName;
            instructor.Headline = dto.Headline;
            instructor.Bio = dto.Bio;
            instructor.ProfilePictureUrl = dto.ProfilePictureUrl;
            instructor.WebsiteUrl = dto.WebsiteUrl;
            instructor.LinkedInUrl = dto.LinkedInUrl;
            instructor.GitHubUrl = dto.GitHubUrl;
            instructor.YouTubeUrl = dto.YouTubeUrl;

            _context.Instructors.Update(instructor);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<InstructorProfileDto?> GetPublicProfileAsync(int instructorId)
        {
            var instructor = await _context.Instructors
                .Include(i => i.Courses.Where(c => c.IsPublished))
                    .ThenInclude(c => c.Sections)
                        .ThenInclude(s => s.Lessons)
                .FirstOrDefaultAsync(i => i.Id == instructorId);

            if (instructor == null) return null;

            var dto = _mapper.Map<InstructorProfileDto>(instructor);
            dto.TotalCourses = instructor.Courses.Count(c => c.IsPublished);

            var courseIds = instructor.Courses.Select(c => c.Id).ToList();
            dto.TotalStudents = await _context.Enrollments.CountAsync(e => courseIds.Contains(e.CourseId));

            return dto;
        }

        // ================= Instructor Courses =================

        public async Task<IEnumerable<CourseDto>> GetMyCoursesAsync(int instructorId)
        {
            var courses = await _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Sections.OrderBy(s => s.OrderIndex))
                    .ThenInclude(s => s.Lessons.OrderBy(l => l.OrderIndex))
                .Include(c => c.Lessons.OrderBy(l => l.OrderIndex))
                .Where(c => c.InstructorId == instructorId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<CourseDto>>(courses);
        }

        public async Task<CourseDto?> GetMyCourseByIdAsync(int instructorId, int courseId)
        {
            var course = await _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Sections.OrderBy(s => s.OrderIndex))
                    .ThenInclude(s => s.Lessons.OrderBy(l => l.OrderIndex))
                .Include(c => c.Lessons.OrderBy(l => l.OrderIndex))
                .FirstOrDefaultAsync(c => c.Id == courseId && c.InstructorId == instructorId);

            return course == null ? null : _mapper.Map<CourseDto>(course);
        }

        public async Task<CourseDto> CreateCourseAsync(int instructorId, CreateCourseDto dto)
        {
            var course = _mapper.Map<Course>(dto);
            course.InstructorId = instructorId;
            course.IsPublished = false;
            course.ApprovalStatus = "Draft";

            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();

            return _mapper.Map<CourseDto>(course);
        }

        public async Task<bool> UpdateCourseAsync(int instructorId, int courseId, CreateCourseDto dto)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId && c.InstructorId == instructorId);
            if (course == null) return false;

            course.Title = dto.Title;
            course.Description = dto.Description;
            course.Category = dto.Category;

            _context.Courses.Update(course);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteCourseAsync(int instructorId, int courseId)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId && c.InstructorId == instructorId);
            if (course == null) return false;

            _context.Courses.Remove(course);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> SubmitForReviewAsync(int instructorId, int courseId)
        {
            var course = await _context.Courses
                .Include(c => c.Lessons)
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Lessons)
                .FirstOrDefaultAsync(c => c.Id == courseId && c.InstructorId == instructorId);

            if (course == null) return false;

            var hasLessons = course.Lessons.Any() || course.Sections.Any(s => s.Lessons.Any());
            if (!hasLessons)
            {
                throw new InvalidOperationException("Cannot submit course for review with no lessons. Please add at least one lesson.");
            }

            course.ApprovalStatus = "PendingReview";
            course.RejectionReason = null;

            _context.Courses.Update(course);
            return await _context.SaveChangesAsync() > 0;
        }

        // ================= Students Enrolled =================

        public async Task<IEnumerable<InstructorStudentDto>> GetMyStudentsAsync(int instructorId)
        {
            var enrollments = await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Where(e => e.Course.InstructorId == instructorId)
                .OrderByDescending(e => e.EnrollmentDate)
                .ToListAsync();

            return enrollments.Select(e => new InstructorStudentDto
            {
                StudentId = e.StudentId,
                FullName = e.Student.FullName,
                Email = e.Student.Email,
                CourseId = e.CourseId,
                CourseTitle = e.Course.Title,
                EnrollmentDate = e.EnrollmentDate,
                ProgressPercentage = e.ProgressPercentage,
                Status = e.Status
            });
        }

        // ================= Sections & Lessons =================

        public async Task<SectionDto?> AddSectionAsync(int instructorId, int courseId, CreateSectionDto dto)
        {
            var course = await _context.Courses
                .Include(c => c.Sections)
                .FirstOrDefaultAsync(c => c.Id == courseId && c.InstructorId == instructorId);

            if (course == null) return null;

            var section = _mapper.Map<Section>(dto);
            section.CourseId = courseId;
            if (section.OrderIndex == 0)
            {
                section.OrderIndex = course.Sections.Count + 1;
            }

            await _context.Sections.AddAsync(section);
            await _context.SaveChangesAsync();

            return _mapper.Map<SectionDto>(section);
        }

        public async Task<bool> UpdateSectionAsync(int instructorId, int sectionId, CreateSectionDto dto)
        {
            var section = await _context.Sections
                .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.Id == sectionId && s.Course.InstructorId == instructorId);

            if (section == null) return false;

            section.Title = dto.Title;
            if (dto.OrderIndex > 0)
            {
                section.OrderIndex = dto.OrderIndex;
            }

            _context.Sections.Update(section);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteSectionAsync(int instructorId, int sectionId)
        {
            var section = await _context.Sections
                .Include(s => s.Course)
                .Include(s => s.Lessons)
                .FirstOrDefaultAsync(s => s.Id == sectionId && s.Course.InstructorId == instructorId);

            if (section == null) return false;

            if (section.Lessons.Any())
            {
                _context.Lessons.RemoveRange(section.Lessons);
            }

            _context.Sections.Remove(section);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<LessonDto?> AddLessonToSectionAsync(int instructorId, int sectionId, CreateLessonDto dto)
        {
            var section = await _context.Sections
                .Include(s => s.Course)
                .Include(s => s.Lessons)
                .FirstOrDefaultAsync(s => s.Id == sectionId && s.Course.InstructorId == instructorId);

            if (section == null) return null;

            var lesson = _mapper.Map<Lesson>(dto);
            lesson.SectionId = sectionId;
            lesson.CourseId = section.CourseId;
            if (lesson.OrderIndex == 0)
            {
                lesson.OrderIndex = section.Lessons.Count + 1;
            }

            await _context.Lessons.AddAsync(lesson);
            await _context.SaveChangesAsync();

            return _mapper.Map<LessonDto>(lesson);
        }

        public async Task<bool> UpdateLessonAsync(int instructorId, int lessonId, CreateLessonDto dto)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.Id == lessonId && (l.Course!.InstructorId == instructorId || l.Section!.Course.InstructorId == instructorId));

            if (lesson == null) return false;

            lesson.Title = dto.Title;
            lesson.Content = dto.Content;
            lesson.LessonType = string.IsNullOrWhiteSpace(dto.LessonType) ? "Video" : dto.LessonType;
            lesson.VideoUrl = dto.VideoUrl;
            lesson.DurationMinutes = dto.DurationMinutes;
            lesson.IsFreePreview = dto.IsFreePreview;
            lesson.ResourceUrl = dto.ResourceUrl;

            if (dto.SectionId.HasValue && dto.SectionId.Value > 0)
            {
                lesson.SectionId = dto.SectionId.Value;
            }

            _context.Lessons.Update(lesson);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteLessonAsync(int instructorId, int lessonId)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.Id == lessonId && (l.Course!.InstructorId == instructorId || l.Section!.Course.InstructorId == instructorId));

            if (lesson == null) return false;

            _context.Lessons.Remove(lesson);
            return await _context.SaveChangesAsync() > 0;
        }

        // ================= Admin Approval Workflow =================

        public async Task<IEnumerable<CourseDto>> GetPendingReviewCoursesAsync()
        {
            var courses = await _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Lessons)
                .Include(c => c.Lessons)
                .Where(c => c.ApprovalStatus == "PendingReview")
                .ToListAsync();

            return _mapper.Map<IEnumerable<CourseDto>>(courses);
        }

        public async Task<bool> ApproveCourseAsync(int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return false;

            course.ApprovalStatus = "Approved";
            course.IsPublished = true;
            course.RejectionReason = null;

            _context.Courses.Update(course);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RejectCourseAsync(int courseId, string reason)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return false;

            course.ApprovalStatus = "Rejected";
            course.IsPublished = false;
            course.RejectionReason = reason;

            _context.Courses.Update(course);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
