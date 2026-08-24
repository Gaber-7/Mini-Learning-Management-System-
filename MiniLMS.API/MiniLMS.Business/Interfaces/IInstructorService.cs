using MiniLMS.Business.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniLMS.Business.Interfaces
{
    public interface IInstructorService
    {
        // الملف الشخصي
        Task<InstructorProfileDto?> GetProfileAsync(int instructorId);
        Task<bool> UpdateProfileAsync(int instructorId, UpdateInstructorProfileDto dto);
        Task<InstructorProfileDto?> GetPublicProfileAsync(int instructorId);

        // إدارة كورسات المدرب
        Task<IEnumerable<CourseDto>> GetMyCoursesAsync(int instructorId);
        Task<CourseDto?> GetMyCourseByIdAsync(int instructorId, int courseId);
        Task<CourseDto> CreateCourseAsync(int instructorId, CreateCourseDto dto);
        Task<bool> UpdateCourseAsync(int instructorId, int courseId, CreateCourseDto dto);
        Task<bool> DeleteCourseAsync(int instructorId, int courseId);
        Task<bool> SubmitForReviewAsync(int instructorId, int courseId);

        // الطلاب المسجلين لدى المدرب
        Task<IEnumerable<InstructorStudentDto>> GetMyStudentsAsync(int instructorId);

        // إدارة الفصول والدروس الخاصة بكورس المدرب
        Task<SectionDto?> AddSectionAsync(int instructorId, int courseId, CreateSectionDto dto);
        Task<bool> UpdateSectionAsync(int instructorId, int sectionId, CreateSectionDto dto);
        Task<bool> DeleteSectionAsync(int instructorId, int sectionId);
        Task<LessonDto?> AddLessonToSectionAsync(int instructorId, int sectionId, CreateLessonDto dto);
        Task<bool> UpdateLessonAsync(int instructorId, int lessonId, CreateLessonDto dto);
        Task<bool> DeleteLessonAsync(int instructorId, int lessonId);

        // مراجعة واعتماد الآدمن (Admin Approval)
        Task<IEnumerable<CourseDto>> GetPendingReviewCoursesAsync();
        Task<bool> ApproveCourseAsync(int courseId);
        Task<bool> RejectCourseAsync(int courseId, string reason);
    }
}
