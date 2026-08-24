using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniLMS.Data.Models
{
    public class Lesson
    {
        public int Id { get; set; }

        public int? CourseId { get; set; }

        public int? SectionId { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        [StringLength(50)]
        public string LessonType { get; set; } = "Video"; // "Video", "Article", "Resource"

        public string? VideoUrl { get; set; }

        public int DurationMinutes { get; set; } = 0;

        public bool IsFreePreview { get; set; } = false;

        public string? ResourceUrl { get; set; }

        public int OrderIndex { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course? Course { get; set; }

        [ForeignKey("SectionId")]
        public virtual Section? Section { get; set; }

        public virtual ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();

        public virtual ICollection<LessonQuestion> Questions { get; set; } = new List<LessonQuestion>();
    }
}