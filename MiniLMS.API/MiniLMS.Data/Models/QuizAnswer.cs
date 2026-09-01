using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenAlpha.Data.Models
{
    public class QuizAnswer
    {
        public int Id { get; set; }

        [Required]
        public int AttemptId { get; set; }

        [Required]
        public int QuestionId { get; set; }

        public int? SelectedOptionId { get; set; }

        public string? AnswerText { get; set; }

        public bool IsCorrect { get; set; } = false;

        [ForeignKey("AttemptId")]
        public virtual QuizAttempt Attempt { get; set; } = null!;

        [ForeignKey("QuestionId")]
        public virtual QuizQuestion Question { get; set; } = null!;

        [ForeignKey("SelectedOptionId")]
        public virtual QuizOption? SelectedOption { get; set; }
    }
}
