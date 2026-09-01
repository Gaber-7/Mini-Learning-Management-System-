using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenAlpha.Data.Models
{
    public class Quiz
    {
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }

        public int? SectionId { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int PassingScorePercentage { get; set; } = 70;

        public int? TimeLimitMinutes { get; set; }

        public int OrderIndex { get; set; } = 0;

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; } = null!;

        [ForeignKey("SectionId")]
        public virtual Section? Section { get; set; }

        public virtual ICollection<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();

        public virtual ICollection<QuizAttempt> Attempts { get; set; } = new List<QuizAttempt>();
    }
}
