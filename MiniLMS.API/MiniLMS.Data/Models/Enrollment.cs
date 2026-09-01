using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenAlpha.Data.Models
{
    public class Enrollment
    {
        public int Id { get; set; } 

        [Required]
        public int StudentId { get; set; } 

        [Required]
        public int CourseId { get; set; }

        public DateTime EnrollmentDate { get; set; } = DateTime.UtcNow; 

        //  "NotStarted", "InProgress", "Completed"
        [Required]
        public string Status { get; set; } = "NotStarted"; 

        [Range(0, 100)]
        [Precision(18, 2)]
        public decimal ProgressPercentage { get; set; } 

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; } = null!;

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; } = null!;

        // ﬁ«∆„…   »⁄  ﬁœ„ «·œ—Ê” ·Â–« «·«‘ —«ﬂ
        public ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();
    }
}
