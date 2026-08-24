using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniLMS.Data.Models
{
    public class LessonQuestion
    {
        public int Id { get; set; }

        [Required]
        public int LessonId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public int? VideoTimestampSeconds { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsResolved { get; set; } = false;

        public int UpvotesCount { get; set; } = 0;

        [ForeignKey("LessonId")]
        public virtual Lesson Lesson { get; set; } = null!;

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; } = null!;

        public virtual ICollection<LessonReply> Replies { get; set; } = new List<LessonReply>();
    }
}
