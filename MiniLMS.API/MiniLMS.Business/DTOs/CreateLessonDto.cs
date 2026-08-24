using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniLMS.Business.DTOs
{
    public class CreateLessonDto
    {
        public int? SectionId { get; set; }

        public int? CourseId { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public string LessonType { get; set; } = "Video"; // "Video", "Article", "Resource"

        public string? VideoUrl { get; set; }

        public int DurationMinutes { get; set; } = 0;

        public bool IsFreePreview { get; set; } = false;

        public string? ResourceUrl { get; set; }

        public int OrderIndex { get; set; }
    }
}
