using MiniLMS.Business.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniLMS.Business.Interfaces
{
    public interface IAssignmentService
    {
        Task<IEnumerable<AssignmentDto>> GetCourseAssignmentsAsync(int courseId, int? studentId = null);
        Task<AssignmentDto?> GetAssignmentByIdAsync(int assignmentId, int? studentId = null);
        Task<AssignmentDto> CreateAssignmentAsync(int courseId, CreateAssignmentDto dto);
        Task<bool> UpdateAssignmentAsync(int assignmentId, CreateAssignmentDto dto);
        Task<bool> DeleteAssignmentAsync(int assignmentId);
        Task<AssignmentSubmissionDto> SubmitAssignmentAsync(int assignmentId, int studentId, SubmitAssignmentDto dto);
        Task<bool> GradeSubmissionAsync(int submissionId, GradeAssignmentDto dto);
        Task<IEnumerable<AssignmentSubmissionDto>> GetAssignmentSubmissionsAsync(int assignmentId);
    }
}
