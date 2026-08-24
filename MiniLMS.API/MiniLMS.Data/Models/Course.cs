using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniLMS.Data.Models
{
    public class Course
    {
        public int Id { get; set; }

        public int? InstructorId { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        public bool IsPublished { get; set; }

        [StringLength(50)]
        public string ApprovalStatus { get; set; } = "Approved"; // "Draft", "PendingReview", "Approved", "Rejected"

        public string? RejectionReason { get; set; }

        [Column(TypeName = "decimal(3, 2)")]
        public decimal AverageRating { get; set; } = 0;

        public int ReviewsCount { get; set; } = 0;

        [ForeignKey("InstructorId")]
        public virtual Instructor? Instructor { get; set; }

        public virtual ICollection<Section> Sections { get; set; } = new List<Section>();

        public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

        public virtual ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();

        public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

        public virtual ICollection<CourseReview> Reviews { get; set; } = new List<CourseReview>();
    }
}
