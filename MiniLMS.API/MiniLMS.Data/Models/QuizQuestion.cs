using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenAlpha.Data.Models
{
    public class QuizQuestion
    {
        public int Id { get; set; }

        [Required]
        public int QuizId { get; set; }

        [Required]
        public string QuestionText { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string QuestionType { get; set; } = "MCQ"; // "MCQ", "TrueFalse", "ShortAnswer"

        public int Points { get; set; } = 1;

        public string? Explanation { get; set; }

        public int OrderIndex { get; set; } = 0;

        [ForeignKey("QuizId")]
        public virtual Quiz Quiz { get; set; } = null!;

        public virtual ICollection<QuizOption> Options { get; set; } = new List<QuizOption>();

        public virtual ICollection<QuizAnswer> Answers { get; set; } = new List<QuizAnswer>();
    }
}
