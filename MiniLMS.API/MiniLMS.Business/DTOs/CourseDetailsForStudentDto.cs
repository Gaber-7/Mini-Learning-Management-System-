using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniLMS.Business.DTOs
{
    public class CourseDetailsForStudentDto
    {
        public int CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsEnrolled { get; set; }
        public decimal ProgressPercentage { get; set; }
        public string Status { get; set; } = "NotStarted";
        public List<LessonProgressDto> Lessons { get; set; } = new();
    }
}
