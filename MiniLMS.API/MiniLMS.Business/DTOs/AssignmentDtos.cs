using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenAlpha.Business.DTOs
{
    public class AssignmentSubmissionDto
    {
        public int Id { get; set; }
        public int AssignmentId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public DateTime SubmissionDate { get; set; }
        public string? FileUrl { get; set; }
        public string? StudentNotes { get; set; }
        public decimal? Grade { get; set; }
        public string? InstructorFeedback { get; set; }
        public string Status { get; set; } = "Submitted";
    }

    public class AssignmentDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int? SectionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? AttachmentUrl { get; set; }
        public int MaxScore { get; set; } = 100;
        public DateTime? DueDate { get; set; }
        public int OrderIndex { get; set; }
        public bool IsSubmittedByStudent { get; set; }
        public AssignmentSubmissionDto? MySubmission { get; set; }
        public int TotalSubmissionsCount { get; set; }
    }

    public class CreateAssignmentDto
    {
        public int? SectionId { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public string? AttachmentUrl { get; set; }
        public int MaxScore { get; set; } = 100;
        public DateTime? DueDate { get; set; }
        public int OrderIndex { get; set; }
    }

    public class SubmitAssignmentDto
    {
        public string? FileUrl { get; set; }
        public string? StudentNotes { get; set; }
    }

    public class GradeAssignmentDto
    {
        [Required]
        public decimal Grade { get; set; }

        public string? InstructorFeedback { get; set; }
    }
}
