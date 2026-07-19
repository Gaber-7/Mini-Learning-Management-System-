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
            // الطلاب يشاهدون الكورسات المنشورة فقط
            var query = _context.Courses.Where(c => c.IsPublished).AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(c => c.Title.Contains(search) || c.Description.Contains(search));

            if (!string.IsNullOrEmpty(category))
                query = query.Where(c => c.Category == category);

            var courses = await query.Include(c => c.Lessons).ToListAsync();
            return _mapper.Map<IEnumerable<CourseDto>>(courses);
        }

        public async Task<EnrollmentDto> EnrollInCourseAsync(int studentId, int courseId)
        {
            // التأكد من وجود الكورس
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null)
                throw new InvalidOperationException("Course not found.");

            // التأكد أن الكورس منشور
            if (!course.IsPublished)
                throw new InvalidOperationException("Course is not published yet.");

            // التأكد من عدم الاشتراك المكرر
            var existingEnrollment = await _context.Enrollments
                .AnyAsync(e => e.StudentId == studentId &&
                               e.CourseId == courseId);

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
                .Where(e => e.StudentId == studentId)
                .ToListAsync();

            return _mapper.Map<IEnumerable<EnrollmentDto>>(enrollments);
        }

        public async Task<CourseDetailsForStudentDto?> GetCourseDetailsForStudentAsync(int studentId, int courseId)
        {
            var course = await _context.Courses
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.Id == courseId && c.IsPublished);

            if (course == null) return null;

            var enrollment = await _context.Enrollments
                .Include(e => e.LessonProgresses)
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);

            var dto = new CourseDetailsForStudentDto
            {
                CourseId = course.Id,
                Title = course.Title,
                Description = course.Description,
                Category = course.Category,
                IsEnrolled = enrollment != null
            };

            if (enrollment != null)
            {
                dto.ProgressPercentage = enrollment.ProgressPercentage;
                dto.Status = enrollment.Status;

                // ربط الدروس بحالة اكتمالها للطالب المشترك
                dto.Lessons = course.Lessons.OrderBy(l => l.OrderIndex).Select(l => new LessonProgressDto
                {
                    LessonId = l.Id,
                    LessonTitle = l.Title,
                    IsCompleted = enrollment.LessonProgresses.Any(lp => lp.LessonId == l.Id && lp.IsCompleted),
                    CompletedDate = enrollment.LessonProgresses.FirstOrDefault(lp => lp.LessonId == l.Id)?.CompletedDate
                }).ToList();
            }
            else
            {
                dto.Lessons = course.Lessons.OrderBy(l => l.OrderIndex).Select(l => new LessonProgressDto
                {
                    LessonId = l.Id,
                    LessonTitle = l.Title,
                    IsCompleted = false
                }).ToList();
            }

            return dto;
        }

        public async Task<bool> CompleteLessonAsync(int studentId, int enrollmentId, int lessonId)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.LessonProgresses)
                .Include(e => e.Course)
                .ThenInclude(c => c.Lessons)
                .FirstOrDefaultAsync(e => e.Id == enrollmentId && e.StudentId == studentId);

            if (enrollment == null) return false;

            // التحقق من أن الدرس ينتمي للكورس المشترك به بالفعل
            var lessonExists = enrollment.Course.Lessons.Any(l => l.Id == lessonId);
            if (!lessonExists) return false;

            var progress = enrollment.LessonProgresses.FirstOrDefault(lp => lp.LessonId == lessonId);
            if (progress == null)
            {
                progress = new LessonProgress
                {
                    EnrollmentId = enrollmentId,
                    LessonId = lessonId,
                    IsCompleted = true,
                    CompletedDate = DateTime.UtcNow
                };
                _context.LessonProgresses.Add(progress);
            }
            else
            {
                progress.IsCompleted = true;
                progress.CompletedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // حساب التقدم ونسبة الإكمال وتحديث الحالة تلقائياً
            var totalLessonsCount = enrollment.Course.Lessons.Count;
            var completedLessonsCount = enrollment.LessonProgresses.Count(lp => lp.IsCompleted);

            if (totalLessonsCount > 0)
            {
                enrollment.ProgressPercentage = Math.Round(((decimal)completedLessonsCount / totalLessonsCount) * 100, 2);
            }

            // تحديث الحالة بناءً على النسبة المئوية
            if (enrollment.ProgressPercentage == 0)
                enrollment.Status = "NotStarted";
            else if (enrollment.ProgressPercentage == 100)
                enrollment.Status = "Completed";
            else
                enrollment.Status = "InProgress";

            _context.Enrollments.Update(enrollment);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
