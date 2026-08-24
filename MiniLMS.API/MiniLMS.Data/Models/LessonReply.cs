using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniLMS.Data.Models
{
    public class LessonReply
    {
        public int Id { get; set; }

        [Required]
        public int QuestionId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsInstructorReply { get; set; } = false;

        public bool IsAcceptedAnswer { get; set; } = false;

        public int UpvotesCount { get; set; } = 0;

        [ForeignKey("QuestionId")]
        public virtual LessonQuestion Question { get; set; } = null!;

        [ForeignKey("UserId")]
        public virtual User User { get; set; } = null!;
    }
}
