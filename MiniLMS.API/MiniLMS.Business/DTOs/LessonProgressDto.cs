using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniLMS.Business.DTOs
{
    public class LessonProgressDto
    {
        public int LessonId { get; set; }
        public int? SectionId { get; set; }
        public string LessonTitle { get; set; } = string.Empty;
        public string LessonType { get; set; } = "Video";
        public string? VideoUrl { get; set; }
        public int DurationMinutes { get; set; }
        public bool IsFreePreview { get; set; }
        public string? ResourceUrl { get; set; }
        public string? Content { get; set; }
        public int OrderIndex { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedDate { get; set; }
        public int LastWatchedSeconds { get; set; } = 0;
        public decimal WatchPercentage { get; set; } = 0;
    }
}
