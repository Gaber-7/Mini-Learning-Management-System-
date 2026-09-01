using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenAlpha.Data.Models
{
    public class AssignmentSubmission
    {
        public int Id { get; set; }

        [Required]
        public int AssignmentId { get; set; }

        [Required]
        public int StudentId { get; set; }

        public DateTime SubmissionDate { get; set; } = DateTime.UtcNow;

        public string? FileUrl { get; set; }

        public string? StudentNotes { get; set; }

        [Column(TypeName = "decimal(5, 2)")]
        public decimal? Grade { get; set; }

        public string? InstructorFeedback { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Submitted"; // "Submitted", "Graded", "ResubmissionRequested"

        [ForeignKey("AssignmentId")]
        public virtual Assignment Assignment { get; set; } = null!;

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; } = null!;
    }
}
