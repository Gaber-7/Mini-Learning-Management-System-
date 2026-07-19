using MiniLMS.Business.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniLMS.Business.Interfaces
{
    public interface IStudentService
    {
        Task<IEnumerable<CourseDto>> GetAvailableCoursesForStudentsAsync(string? search, string? category);
        Task<EnrollmentDto> EnrollInCourseAsync(int studentId, int courseId);
        Task<IEnumerable<EnrollmentDto>> GetMyEnrollmentsAsync(int studentId);
        Task<CourseDetailsForStudentDto?> GetCourseDetailsForStudentAsync(int studentId, int courseId);
        Task<bool> CompleteLessonAsync(int studentId, int enrollmentId, int lessonId);
    }
}
