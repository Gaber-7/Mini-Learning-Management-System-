using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniLMS.Data.Models
{
    public class QuizAttempt
    {
        public int Id { get; set; }

        [Required]
        public int QuizId { get; set; }

        [Required]
        public int StudentId { get; set; }

        public int Score { get; set; } = 0;

        public int TotalPoints { get; set; } = 0;

        [Column(TypeName = "decimal(5, 2)")]
        public decimal Percentage { get; set; } = 0;

        public bool IsPassed { get; set; } = false;

        public DateTime AttemptDate { get; set; } = DateTime.UtcNow;

        [ForeignKey("QuizId")]
        public virtual Quiz Quiz { get; set; } = null!;

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; } = null!;

        public virtual ICollection<QuizAnswer> Answers { get; set; } = new List<QuizAnswer>();
    }
}
