using System;
using System.Collections.Generic;

namespace GenAlpha.Business.DTOs
{
    public class AiExplainRequestDto
    {
        public string Prompt { get; set; } = string.Empty;
        public string? LessonTitle { get; set; }
        public string? LessonContext { get; set; }
        public string Language { get; set; } = "Arabic"; // "Arabic", "English"
    }

    public class AiSummarizeRequestDto
    {
        public string LessonTitle { get; set; } = string.Empty;
        public string? LessonContent { get; set; }
        public string Language { get; set; } = "Arabic";
    }

    public class AiPracticeQuestionsRequestDto
    {
        public string Topic { get; set; } = string.Empty;
        public string? LessonTitle { get; set; }
        public int QuestionCount { get; set; } = 3;
        public string Difficulty { get; set; } = "Medium"; // "Easy", "Medium", "Hard"
    }

    public class AiResponseDto
    {
        public bool Success { get; set; } = true;
        public string Output { get; set; } = string.Empty;
        public string? ModelUsed { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class AiPracticeQuestionDto
    {
        public int Id { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public List<string> Options { get; set; } = new List<string>();
        public int CorrectOptionIndex { get; set; }
        public string Explanation { get; set; } = string.Empty;
    }
}
