using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenAlpha.Business.DTOs
{
    public class CourseDto
    {
        public int Id { get; set; }
        public int? InstructorId { get; set; }
        public string? InstructorName { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsPublished { get; set; }
        public string ApprovalStatus { get; set; } = "Approved"; // "Draft", "PendingReview", "Approved", "Rejected"
        public string? RejectionReason { get; set; }
        public decimal AverageRating { get; set; } = 0;
        public int ReviewsCount { get; set; } = 0;
        public List<SectionDto> Sections { get; set; } = new();
        public List<LessonDto> Lessons { get; set; } = new();
    }
}
