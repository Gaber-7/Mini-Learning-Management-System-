using GenAlpha.Business.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenAlpha.Business.Interfaces
{
    public interface IAdminUserService
    {
        // Students
        Task<IEnumerable<StudentListItemDto>> GetAllStudentsAsync();
        Task<StudentListItemDto?> GetStudentByIdAsync(int studentId);
        Task<StudentListItemDto> CreateStudentAsync(CreateStudentDto dto);
        Task<bool> UpdateStudentAsync(int studentId, UpdateStudentDto dto);
        Task<bool> DeleteStudentAsync(int studentId);

        // Instructors
        Task<IEnumerable<InstructorListItemDto>> GetAllInstructorsAsync();
        Task<InstructorListItemDto?> GetInstructorByIdAsync(int instructorId);
        Task<InstructorListItemDto> CreateInstructorAsync(CreateInstructorDto dto);
        Task<bool> UpdateInstructorAsync(int instructorId, UpdateInstructorAdminDto dto);
        Task<bool> DeleteInstructorAsync(int instructorId);

        // Reviews
        Task<IEnumerable<AdminReviewItemDto>> GetAllReviewsAsync();
        Task<bool> ToggleReviewApprovalAsync(int reviewId);
        Task<bool> DeleteReviewAsync(int reviewId);
    }
}
