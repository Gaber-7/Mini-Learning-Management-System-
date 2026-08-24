using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniLMS.Data.Models
{
    public class Assignment
    {
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }

        public int? SectionId { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public string? AttachmentUrl { get; set; }

        public int MaxScore { get; set; } = 100;

        public DateTime? DueDate { get; set; }

        public int OrderIndex { get; set; } = 0;

        [ForeignKey("CourseId")]
        public virtual Course Course { get; set; } = null!;

        [ForeignKey("SectionId")]
        public virtual Section? Section { get; set; }

        public virtual ICollection<AssignmentSubmission> Submissions { get; set; } = new List<AssignmentSubmission>();
    }
}
