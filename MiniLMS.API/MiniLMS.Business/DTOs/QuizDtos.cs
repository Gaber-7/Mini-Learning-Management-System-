using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniLMS.Business.DTOs
{
    public class QuizOptionDto
    {
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public string OptionText { get; set; } = string.Empty;
        public bool? IsCorrect { get; set; } // Hide from student before attempt
    }

    public class CreateQuizOptionDto
    {
        [Required]
        [StringLength(300)]
        public string OptionText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; } = false;
    }

    public class QuizQuestionDto
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = "MCQ";
        public int Points { get; set; } = 1;
        public string? Explanation { get; set; }
        public int OrderIndex { get; set; }
        public List<QuizOptionDto> Options { get; set; } = new();
    }

    public class CreateQuizQuestionDto
    {
        [Required]
        public string QuestionText { get; set; } = string.Empty;
        public string QuestionType { get; set; } = "MCQ";
        public int Points { get; set; } = 1;
        public string? Explanation { get; set; }
        public int OrderIndex { get; set; }
        public List<CreateQuizOptionDto> Options { get; set; } = new();
    }

    public class QuizDto
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int? SectionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int PassingScorePercentage { get; set; } = 70;
        public int? TimeLimitMinutes { get; set; }
        public int OrderIndex { get; set; }
        public int TotalQuestions { get; set; }
        public int TotalPoints { get; set; }
        public bool IsPassedByStudent { get; set; }
        public decimal? BestScorePercentage { get; set; }
        public List<QuizQuestionDto> Questions { get; set; } = new();
    }

    public class CreateQuizDto
    {
        public int? SectionId { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }
        public int PassingScorePercentage { get; set; } = 70;
        public int? TimeLimitMinutes { get; set; }
        public int OrderIndex { get; set; }
        public List<CreateQuizQuestionDto> Questions { get; set; } = new();
    }

    public class SubmitQuizAnswerDto
    {
        public int QuestionId { get; set; }
        public int? SelectedOptionId { get; set; }
        public string? AnswerText { get; set; }
    }

    public class SubmitQuizDto
    {
        public List<SubmitQuizAnswerDto> Answers { get; set; } = new();
    }

    public class QuizResultDto
    {
        public int AttemptId { get; set; }
        public int QuizId { get; set; }
        public int Score { get; set; }
        public int TotalPoints { get; set; }
        public decimal Percentage { get; set; }
        public bool IsPassed { get; set; }
        public DateTime AttemptDate { get; set; }
        public List<QuizQuestionResultDto> QuestionsResults { get; set; } = new();
    }

    public class QuizQuestionResultDto
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public int? SelectedOptionId { get; set; }
        public int? CorrectOptionId { get; set; }
        public string? Explanation { get; set; }
    }
}
