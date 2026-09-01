using AutoMapper;
using Microsoft.EntityFrameworkCore;
using GenAlpha.Business.DTOs;
using GenAlpha.Business.Interfaces;
using GenAlpha.Data.Data;
using GenAlpha.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenAlpha.Business.Services
{
    public class StudentService : IStudentService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public StudentService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CourseDto>> GetAvailableCoursesForStudentsAsync(string? search, string? category)
        {
            var query = _context.Courses.Where(c => c.IsPublished).AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(c => c.Title.Contains(search) || c.Description.Contains(search));

            if (!string.IsNullOrEmpty(category))
                query = query.Where(c => c.Category == category);

            var courses = await query
                .Include(c => c.Instructor)
                .Include(c => c.Sections.OrderBy(s => s.OrderIndex))
                    .ThenInclude(s => s.Lessons.OrderBy(l => l.OrderIndex))
                .Include(c => c.Lessons.OrderBy(l => l.OrderIndex))
                .ToListAsync();

            return _mapper.Map<IEnumerable<CourseDto>>(courses);
        }

        public async Task<EnrollmentDto> EnrollInCourseAsync(int studentId, int courseId)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
                throw new InvalidOperationException("Course not found.");

            if (!course.IsPublished)
                throw new InvalidOperationException("Course is not published yet.");

            var existingEnrollment = await _context.Enrollments
                .AnyAsync(e => e.StudentId == studentId && e.CourseId == courseId);

            if (existingEnrollment)
                throw new InvalidOperationException("You are already enrolled in this course.");

            var enrollment = new Enrollment
            {
                StudentId = studentId,
                CourseId = courseId,
                EnrollmentDate = DateTime.UtcNow,
                Status = "NotStarted",
                ProgressPercentage = 0
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            return _mapper.Map<EnrollmentDto>(enrollment);
        }

        public async Task<IEnumerable<EnrollmentDto>> GetMyEnrollmentsAsync(int studentId)
        {
            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Instructor)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Sections)
                        .ThenInclude(s => s.Lessons)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Lessons)
                .Where(e => e.StudentId == studentId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<EnrollmentDto>>(enrollments);
        }

        public async Task<CourseDetailsForStudentDto?> GetCourseDetailsForStudentAsync(int studentId, int courseId)
        {
            var course = await _context.Courses
                .Include(c => c.Instructor)
                .Include(c => c.Sections.OrderBy(s => s.OrderIndex))
                    .ThenInclude(s => s.Lessons.OrderBy(l => l.OrderIndex))
                .Include(c => c.Lessons.OrderBy(l => l.OrderIndex))
                .Include(c => c.Quizzes)
                .Include(c => c.Assignments)
                .FirstOrDefaultAsync(c => c.Id == courseId && c.IsPublished);

            if (course == null) return null;

            var enrollment = await _context.Enrollments
                .Include(e => e.LessonProgresses)
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);

            var isEnrolled = enrollment != null;

            var allCourseLessons = course.Sections
                .SelectMany(s => s.Lessons)
                .Union(course.Lessons)
                .Distinct()
                .OrderBy(l => l.OrderIndex)
                .ToList();

            var totalDuration = allCourseLessons.Sum(l => l.DurationMinutes);
            var completedLessonIds = enrollment != null 
                ? enrollment.LessonProgresses.Where(lp => lp.IsCompleted).Select(lp => lp.LessonId).ToHashSet()
                : new HashSet<int>();

            var dto = new CourseDetailsForStudentDto
            {
                CourseId = course.Id,
                InstructorId = course.InstructorId,
                InstructorName = course.Instructor?.FullName,
                Title = course.Title,
                Description = course.Description,
                Category = course.Category,
                IsEnrolled = isEnrolled,
                ProgressPercentage = enrollment?.ProgressPercentage ?? 0,
                Status = enrollment?.Status ?? "NotStarted",
                AverageRating = course.AverageRating,
                ReviewsCount = course.ReviewsCount,
                TotalDurationMinutes = totalDuration,
                TotalLessonsCount = allCourseLessons.Count,
                CompletedLessonsCount = completedLessonIds.Count,
                QuizzesCount = course.Quizzes.Count,
                AssignmentsCount = course.Assignments.Count,
                Sections = new List<StudentSectionDto>(),
                Lessons = new List<LessonProgressDto>()
            };

            foreach (var section in course.Sections)
            {
                var secDto = new StudentSectionDto
                {
                    SectionId = section.Id,
                    Title = section.Title,
                    OrderIndex = section.OrderIndex,
                    Lessons = section.Lessons.OrderBy(l => l.OrderIndex).Select(l => MapToStudentLesson(l, isEnrolled, enrollment)).ToList()
                };
                dto.Sections.Add(secDto);
            }

            var standaloneLessons = course.Lessons.Where(l => l.SectionId == null).ToList();
            if (standaloneLessons.Any())
            {
                if (dto.Sections.Count == 0)
                {
                    dto.Sections.Add(new StudentSectionDto
                    {
                        SectionId = 0,
                        Title = "General Lessons",
                        OrderIndex = 1,
                        Lessons = standaloneLessons.Select(l => MapToStudentLesson(l, isEnrolled, enrollment)).ToList()
                    });
                }
                else
                {
                    var defaultSec = new StudentSectionDto
                    {
                        SectionId = 0,
                        Title = "Additional Lessons",
                        OrderIndex = dto.Sections.Count + 1,
                        Lessons = standaloneLessons.Select(l => MapToStudentLesson(l, isEnrolled, enrollment)).ToList()
                    };
                    dto.Sections.Add(defaultSec);
                }
            }

            dto.Lessons = dto.Sections.SelectMany(s => s.Lessons).ToList();

            return dto;
        }

        private static LessonProgressDto MapToStudentLesson(Lesson l, bool isEnrolled, Enrollment? enrollment)
        {
            var progress = enrollment?.LessonProgresses.FirstOrDefault(lp => lp.LessonId == l.Id);
            var isCompleted = progress?.IsCompleted == true;
            var canAccessContent = isEnrolled || l.IsFreePreview;

            return new LessonProgressDto
            {
                LessonId = l.Id,
                SectionId = l.SectionId,
                LessonTitle = l.Title,
                LessonType = l.LessonType,
                DurationMinutes = l.DurationMinutes,
                IsFreePreview = l.IsFreePreview,
                OrderIndex = l.OrderIndex,
                IsCompleted = isCompleted,
                CompletedDate = progress?.CompletedDate,
                LastWatchedSeconds = progress?.LastWatchedSeconds ?? 0,
                WatchPercentage = progress?.WatchPercentage ?? 0,
                Content = canAccessContent ? l.Content : null,
                VideoUrl = canAccessContent ? l.VideoUrl : null,
                ResourceUrl = canAccessContent ? l.ResourceUrl : null
            };
        }

        public async Task<bool> CompleteLessonAsync(int studentId, int enrollmentId, int lessonId)
        {
            return await UpdateWatchProgressAsync(studentId, enrollmentId, lessonId, new UpdateLessonProgressDto
            {
                ForceCompleted = true,
                WatchPercentage = 100
            }) != null;
        }

        public async Task<LessonProgressDto?> UpdateWatchProgressAsync(int studentId, int enrollmentId, int lessonId, UpdateLessonProgressDto dto)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.LessonProgresses)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Sections)
                        .ThenInclude(s => s.Lessons)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Lessons)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId && e.StudentId == studentId);

            if (enrollment == null) return null;

            var allLessons = enrollment.Course.Sections
                .SelectMany(s => s.Lessons)
                .Union(enrollment.Course.Lessons)
                .Distinct()
                .ToList();

            var lesson = allLessons.FirstOrDefault(l => l.Id == lessonId);
            if (lesson == null) return null;

            var progress = enrollment.LessonProgresses.FirstOrDefault(lp => lp.LessonId == lessonId);
            if (progress == null)
            {
                progress = new LessonProgress
                {
                    EnrollmentId = enrollmentId,
                    LessonId = lessonId,
                    LastWatchedSeconds = dto.LastWatchedSeconds,
                    WatchPercentage = dto.WatchPercentage
                };
                _context.LessonProgresses.Add(progress);
            }
            else
            {
                progress.LastWatchedSeconds = dto.LastWatchedSeconds;
                if (dto.WatchPercentage > progress.WatchPercentage)
                {
                    progress.WatchPercentage = dto.WatchPercentage;
                }
            }

            // Auto complete if watched 80% or more, or if force completed
            if (dto.ForceCompleted == true || progress.WatchPercentage >= 80)
            {
                if (!progress.IsCompleted)
                {
                    progress.IsCompleted = true;
                    progress.CompletedDate = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();

            // Recalculate course overall progress
            var totalLessonsCount = allLessons.Count;
            var completedLessonsCount = enrollment.LessonProgresses.Count(lp => lp.IsCompleted);

            if (totalLessonsCount > 0)
            {
                enrollment.ProgressPercentage = Math.Round(((decimal)completedLessonsCount / totalLessonsCount) * 100, 2);
            }

            if (enrollment.ProgressPercentage == 0)
                enrollment.Status = "NotStarted";
            else if (enrollment.ProgressPercentage >= 100)
            {
                enrollment.ProgressPercentage = 100;
                enrollment.Status = "Completed";
            }
            else
                enrollment.Status = "InProgress";

            _context.Enrollments.Update(enrollment);
            await _context.SaveChangesAsync();

            return MapToStudentLesson(lesson, true, enrollment);
        }
    }
}
