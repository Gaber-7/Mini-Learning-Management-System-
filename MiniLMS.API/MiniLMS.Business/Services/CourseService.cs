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
            var courses = await _context.Courses
                .Include(c => c.Sections.OrderBy(s => s.OrderIndex))
                    .ThenInclude(s => s.Lessons.OrderBy(l => l.OrderIndex))
                .Include(c => c.Lessons.OrderBy(l => l.OrderIndex))
                .ToListAsync();

            return _mapper.Map<IEnumerable<CourseDto>>(courses);
        }

        public async Task<CourseDto?> GetCourseByIdAsync(int id)
        {
            var course = await _context.Courses
                .Include(c => c.Sections.OrderBy(s => s.OrderIndex))
                    .ThenInclude(s => s.Lessons.OrderBy(l => l.OrderIndex))
                .Include(c => c.Lessons.OrderBy(l => l.OrderIndex))
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

        // Sections
        public async Task<SectionDto?> AddSectionToCourseAsync(int courseId, CreateSectionDto dto)
        {
            var course = await _context.Courses.Include(c => c.Sections).FirstOrDefaultAsync(c => c.Id == courseId);
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

        public async Task<bool> UpdateSectionAsync(int sectionId, CreateSectionDto dto)
        {
            var section = await _context.Sections.FindAsync(sectionId);
            if (section == null) return false;

            section.Title = dto.Title;
            if (dto.OrderIndex > 0)
            {
                section.OrderIndex = dto.OrderIndex;
            }

            _context.Sections.Update(section);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteSectionAsync(int sectionId)
        {
            var section = await _context.Sections
                .Include(s => s.Lessons)
                .FirstOrDefaultAsync(s => s.Id == sectionId);

            if (section == null) return false;

            if (section.Lessons.Any())
            {
                _context.Lessons.RemoveRange(section.Lessons);
            }

            _context.Sections.Remove(section);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ReorderSectionsAsync(int courseId, List<int> sectionIdsInOrder)
        {
            var sections = await _context.Sections
                .Where(s => s.CourseId == courseId)
                .ToListAsync();

            if (!sections.Any()) return false;

            for (int i = 0; i < sectionIdsInOrder.Count; i++)
            {
                var section = sections.FirstOrDefault(s => s.Id == sectionIdsInOrder[i]);
                if (section != null)
                {
                    section.OrderIndex = i + 1;
                }
            }

            return await _context.SaveChangesAsync() > 0;
        }

        // Lessons
        public async Task<LessonDto?> AddLessonToSectionAsync(int sectionId, CreateLessonDto dto)
        {
            var section = await _context.Sections
                .Include(s => s.Lessons)
                .FirstOrDefaultAsync(s => s.Id == sectionId);

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

        public async Task<LessonDto?> AddLessonToCourseAsync(int courseId, CreateLessonDto dto)
        {
            var course = await _context.Courses
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null) return null;

            var lesson = _mapper.Map<Lesson>(dto);
            lesson.CourseId = courseId;
            if (dto.SectionId.HasValue && dto.SectionId.Value > 0)
            {
                lesson.SectionId = dto.SectionId.Value;
            }

            if (lesson.OrderIndex == 0)
            {
                lesson.OrderIndex = course.Lessons.Count + 1;
            }

            await _context.Lessons.AddAsync(lesson);
            await _context.SaveChangesAsync();

            return _mapper.Map<LessonDto>(lesson);
        }

        public async Task<bool> UpdateLessonAsync(int lessonId, CreateLessonDto dto)
        {
            var lesson = await _context.Lessons.FindAsync(lessonId);
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

            if (dto.OrderIndex > 0)
            {
                lesson.OrderIndex = dto.OrderIndex;
            }

            _context.Lessons.Update(lesson);
            return await _context.SaveChangesAsync() > 0;
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
                    lesson.OrderIndex = i + 1;
                }
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> ReorderLessonsInSectionAsync(int sectionId, List<int> lessonIdsInOrder)
        {
            var lessons = await _context.Lessons
                .Where(l => l.SectionId == sectionId)
                .ToListAsync();

            if (!lessons.Any()) return false;

            for (int i = 0; i < lessonIdsInOrder.Count; i++)
            {
                var lesson = lessons.FirstOrDefault(l => l.Id == lessonIdsInOrder[i]);
                if (lesson != null)
                {
                    lesson.OrderIndex = i + 1;
                }
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> PublishCourseAsync(int courseId)
        {
            var course = await _context.Courses
                .Include(c => c.Lessons)
                .Include(c => c.Sections)
                    .ThenInclude(s => s.Lessons)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (course == null) return false;

            var hasLessons = course.Lessons.Any() || course.Sections.Any(s => s.Lessons.Any());
            if (!hasLessons)
            {
                throw new InvalidOperationException("Cannot publish a course with no lessons.");
            }

            course.IsPublished = true;
            _context.Courses.Update(course);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
