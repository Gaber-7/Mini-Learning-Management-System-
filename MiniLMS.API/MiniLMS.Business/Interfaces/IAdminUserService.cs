using MiniLMS.Business.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniLMS.Business.Interfaces
{
    public interface IAdminUserService
    {
        Task<IEnumerable<StudentListItemDto>> GetAllStudentsAsync();
        Task<IEnumerable<InstructorListItemDto>> GetAllInstructorsAsync();
        Task<StudentListItemDto?> GetStudentByIdAsync(int studentId);
        Task<InstructorListItemDto?> GetInstructorByIdAsync(int instructorId);
    }
}
