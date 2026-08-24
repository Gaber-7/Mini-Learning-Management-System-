using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniLMS.Data.Models
{
    public class LessonProgress
    {
        public int Id { get; set; }

        [Required]
        public int EnrollmentId { get; set; }

        [Required]
        public int LessonId { get; set; }

        public bool IsCompleted { get; set; } = false;

        public DateTime? CompletedDate { get; set; }

        public int LastWatchedSeconds { get; set; } = 0;

        [Column(TypeName = "decimal(5, 2)")]
        public decimal WatchPercentage { get; set; } = 0;

        [ForeignKey("EnrollmentId")]
        public virtual Enrollment Enrollment { get; set; } = null!;

        [ForeignKey("LessonId")]
        public virtual Lesson Lesson { get; set; } = null!;
    }
}
