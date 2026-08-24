using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniLMS.Business.DTOs
{
    public class LessonDto
    {
        public int Id { get; set; }
        public int? CourseId { get; set; }
        public int? SectionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string LessonType { get; set; } = "Video";
        public string? VideoUrl { get; set; }
        public int DurationMinutes { get; set; }
        public bool IsFreePreview { get; set; }
        public string? ResourceUrl { get; set; }
        public int OrderIndex { get; set; }
    }
}
