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
    public class CourseService : ICourseService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public CourseService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<CourseDto>> GetAllCoursesAsync()
        {
            var courses = await _context.Courses.Include(c => c.Lessons).ToListAsync();
            return _mapper.Map<IEnumerable<CourseDto>>(courses);
        }
         
        public async Task<CourseDto?> GetCourseByIdAsync(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.Id == id);

            return course == null ? null : _mapper.Map<CourseDto>(course);
        }

        public async Task<CourseDto> CreateCourseAsync(CreateCourseDto dto)
        {
            var course = _mapper.Map<Course>(dto);
            await _context.Courses.AddAsync(course);
            await _context.SaveChangesAsync();
            return _mapper.Map<CourseDto>(course);
        }

        public async Task<bool> UpdateCourseAsync(int id, CreateCourseDto dto)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return false;

            _mapper.Map(dto, course);
            _context.Courses.Update(course);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteCourseAsync(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null) return false;

            _context.Courses.Remove(course);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<LessonDto?> AddLessonToCourseAsync(int courseId, CreateLessonDto dto)
        {
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null) return null;

            var lesson = _mapper.Map<Lesson>(dto);
            lesson.CourseId = courseId;

            await _context.Lessons.AddAsync(lesson);
            await _context.SaveChangesAsync();

            return _mapper.Map<LessonDto>(lesson);
        }

        public async Task<bool> RemoveLessonAsync(int lessonId)
        {
            var lesson = await _context.Lessons.FindAsync(lessonId);
            if (lesson == null) return false;

            _context.Lessons.Remove(lesson);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ReorderLessonsAsync(int courseId, List<int> lessonIdsInOrder)
        {
            var lessons = await _context.Lessons
                .Where(l => l.CourseId == courseId)
                .ToListAsync();

            if (!lessons.Any()) return false;

            for (int i = 0; i < lessonIdsInOrder.Count; i++)
            {
                var lesson = lessons.FirstOrDefault(l => l.Id == lessonIdsInOrder[i]);
                if (lesson != null)
                {
                    lesson.OrderIndex = i + 1; // الترتيب يبدأ من 1
                }
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> PublishCourseAsync(int courseId)
        {
            var course = await _context.Courses
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null) return false;

            // قاعدة التحقق: لا ينشر الكورس إلا إذا احتوى على درس واحد على الأقل
            if (!course.Lessons.Any())
            {
                throw new InvalidOperationException("Cannot publish a course with no lessons.");
            }

            course.IsPublished = true;
            _context.Courses.Update(course);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
