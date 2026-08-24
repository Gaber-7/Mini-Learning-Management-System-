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
    public class AssignmentService : IAssignmentService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public AssignmentService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AssignmentDto>> GetCourseAssignmentsAsync(int courseId, int? studentId = null)
        {
            var assignments = await _context.Assignments
                .Include(a => a.Submissions)
                    .ThenInclude(s => s.Student)
                .Where(a => a.CourseId == courseId)
                .OrderBy(a => a.OrderIndex)
                .ToListAsync();

            var dtos = _mapper.Map<List<AssignmentDto>>(assignments);

            if (studentId.HasValue)
            {
                for (int i = 0; i < assignments.Count; i++)
                {
                    var mySub = assignments[i].Submissions.FirstOrDefault(s => s.StudentId == studentId.Value);
                    dtos[i].IsSubmittedByStudent = mySub != null;
                    dtos[i].MySubmission = mySub == null ? null : _mapper.Map<AssignmentSubmissionDto>(mySub);
                }
            }

            return dtos;
        }

        public async Task<AssignmentDto?> GetAssignmentByIdAsync(int assignmentId, int? studentId = null)
        {
            var a = await _context.Assignments
                .Include(x => x.Submissions)
                    .ThenInclude(s => s.Student)
                .FirstOrDefaultAsync(x => x.Id == assignmentId);

            if (a == null) return null;

            var dto = _mapper.Map<AssignmentDto>(a);
            if (studentId.HasValue)
            {
                var mySub = a.Submissions.FirstOrDefault(s => s.StudentId == studentId.Value);
                dto.IsSubmittedByStudent = mySub != null;
                dto.MySubmission = mySub == null ? null : _mapper.Map<AssignmentSubmissionDto>(mySub);
            }

            return dto;
        }

        public async Task<AssignmentDto> CreateAssignmentAsync(int courseId, CreateAssignmentDto dto)
        {
            var assignment = _mapper.Map<Assignment>(dto);
            assignment.CourseId = courseId;
            if (assignment.MaxScore == 0) assignment.MaxScore = 100;

            await _context.Assignments.AddAsync(assignment);
            await _context.SaveChangesAsync();

            return (await GetAssignmentByIdAsync(assignment.Id))!;
        }

        public async Task<bool> UpdateAssignmentAsync(int assignmentId, CreateAssignmentDto dto)
        {
            var assignment = await _context.Assignments.FindAsync(assignmentId);
            if (assignment == null) return false;

            assignment.Title = dto.Title;
            assignment.Description = dto.Description;
            assignment.AttachmentUrl = dto.AttachmentUrl;
            assignment.MaxScore = dto.MaxScore;
            assignment.DueDate = dto.DueDate;
            assignment.SectionId = dto.SectionId;
            if (dto.OrderIndex > 0) assignment.OrderIndex = dto.OrderIndex;

            _context.Assignments.Update(assignment);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAssignmentAsync(int assignmentId)
        {
            var assignment = await _context.Assignments.FindAsync(assignmentId);
            if (assignment == null) return false;

            _context.Assignments.Remove(assignment);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<AssignmentSubmissionDto> SubmitAssignmentAsync(int assignmentId, int studentId, SubmitAssignmentDto dto)
        {
            var existingSub = await _context.AssignmentSubmissions
                .Include(s => s.Student)
                .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == studentId);

            if (existingSub != null)
            {
                existingSub.FileUrl = dto.FileUrl ?? existingSub.FileUrl;
                existingSub.StudentNotes = dto.StudentNotes;
                existingSub.SubmissionDate = DateTime.UtcNow;
                existingSub.Status = "Submitted";

                _context.AssignmentSubmissions.Update(existingSub);
                await _context.SaveChangesAsync();

                return _mapper.Map<AssignmentSubmissionDto>(existingSub);
            }

            var newSub = new AssignmentSubmission
            {
                AssignmentId = assignmentId,
                StudentId = studentId,
                FileUrl = dto.FileUrl,
                StudentNotes = dto.StudentNotes,
                SubmissionDate = DateTime.UtcNow,
                Status = "Submitted"
            };

            await _context.AssignmentSubmissions.AddAsync(newSub);
            await _context.SaveChangesAsync();

            var student = await _context.Students.FindAsync(studentId);
            newSub.Student = student!;

            return _mapper.Map<AssignmentSubmissionDto>(newSub);
        }

        public async Task<bool> GradeSubmissionAsync(int submissionId, GradeAssignmentDto dto)
        {
            var sub = await _context.AssignmentSubmissions.FindAsync(submissionId);
            if (sub == null) return false;

            sub.Grade = dto.Grade;
            sub.InstructorFeedback = dto.InstructorFeedback;
            sub.Status = "Graded";

            _context.AssignmentSubmissions.Update(sub);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<AssignmentSubmissionDto>> GetAssignmentSubmissionsAsync(int assignmentId)
        {
            var submissions = await _context.AssignmentSubmissions
                .Include(s => s.Student)
                .Where(s => s.AssignmentId == assignmentId)
                .OrderByDescending(s => s.SubmissionDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<AssignmentSubmissionDto>>(submissions);
        }
    }
}
