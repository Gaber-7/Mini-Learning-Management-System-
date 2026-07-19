using MiniLMS.Business.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniLMS.Business.Interfaces
{
    public interface ICourseService
    {
        Task<IEnumerable<CourseDto>> GetAllCoursesAsync();
        Task<CourseDto?> GetCourseByIdAsync(int id);
        Task<CourseDto> CreateCourseAsync(CreateCourseDto dto);
        Task<bool> UpdateCourseAsync(int id, CreateCourseDto dto);
        Task<bool> DeleteCourseAsync(int id);

        // إدارة الدروس
        Task<LessonDto?> AddLessonToCourseAsync(int courseId, CreateLessonDto dto);
        Task<bool> RemoveLessonAsync(int lessonId);
        Task<bool> ReorderLessonsAsync(int courseId, List<int> lessonIdsInOrder);

        // نشر الكورس
        Task<bool> PublishCourseAsync(int courseId);
    }
}
