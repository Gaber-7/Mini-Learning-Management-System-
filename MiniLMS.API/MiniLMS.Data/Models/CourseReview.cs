using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniLMS.Data.Models
{
    public class CourseReview
    {
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; } = 5;

        [StringLength(1000)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsApproved { get; set; } = true;

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; } = null!;

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; } = null!;
    }
}
