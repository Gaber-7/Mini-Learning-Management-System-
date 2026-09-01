using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenAlpha.Business.DTOs
{
    public class CourseDetailsForStudentDto
    {
        public int CourseId { get; set; }
        public int? InstructorId { get; set; }
        public string? InstructorName { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsEnrolled { get; set; }
        public decimal ProgressPercentage { get; set; }
        public string Status { get; set; } = "NotStarted";
        public decimal AverageRating { get; set; } = 0;
        public int ReviewsCount { get; set; } = 0;
        public int TotalDurationMinutes { get; set; }
        public int TotalLessonsCount { get; set; }
        public int CompletedLessonsCount { get; set; }
        public int QuizzesCount { get; set; }
        public int AssignmentsCount { get; set; }
        public List<StudentSectionDto> Sections { get; set; } = new();
        public List<LessonProgressDto> Lessons { get; set; } = new();
    }
}
