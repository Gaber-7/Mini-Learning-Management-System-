using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniLMS.Data.Models
{
    public class QuizOption
    {
        public int Id { get; set; }

        [Required]
        public int QuestionId { get; set; }

        [Required]
        [StringLength(300)]
        public string OptionText { get; set; } = string.Empty;

        public bool IsCorrect { get; set; } = false;

        [ForeignKey("QuestionId")]
        public virtual QuizQuestion Question { get; set; } = null!;
    }
}
