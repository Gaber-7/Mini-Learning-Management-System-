using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniLMS.Business.DTOs
{
    public class LessonReplyDto
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public int UserId { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorRole { get; set; } = "Student";
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsInstructorReply { get; set; }
        public bool IsAcceptedAnswer { get; set; }
        public int UpvotesCount { get; set; }
    }

    public class CreateLessonReplyDto
    {
        [Required]
        public string Content { get; set; } = string.Empty;
    }

    public class LessonQuestionDto
    {
        public int Id { get; set; }
        public int LessonId { get; set; }
        public string LessonTitle { get; set; } = string.Empty;
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int? VideoTimestampSeconds { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsResolved { get; set; }
        public int UpvotesCount { get; set; }
        public int RepliesCount { get; set; }
        public List<LessonReplyDto> Replies { get; set; } = new();
    }

    public class CreateLessonQuestionDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        public int? VideoTimestampSeconds { get; set; }
    }
}
