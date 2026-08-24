using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MiniLMS.Business.DTOs;
using MiniLMS.Business.Interfaces;
using MiniLMS.Data.Data;
using MiniLMS.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniLMS.Business.Services
{
    public class QuizService : IQuizService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public QuizService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<QuizDto>> GetCourseQuizzesAsync(int courseId, int? studentId = null)
        {
            var quizzes = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(qq => qq.Options)
                .Include(q => q.Attempts)
                .Where(q => q.CourseId == courseId)
                .OrderBy(q => q.OrderIndex)
                .ToListAsync();

            var dtos = _mapper.Map<List<QuizDto>>(quizzes);

            for (int i = 0; i < quizzes.Count; i++)
            {
                var q = quizzes[i];
                var dto = dtos[i];

                if (studentId.HasValue)
                {
                    var studentAttempts = q.Attempts.Where(a => a.StudentId == studentId.Value).ToList();
                    dto.IsPassedByStudent = studentAttempts.Any(a => a.IsPassed);
                    dto.BestScorePercentage = studentAttempts.Any() ? studentAttempts.Max(a => a.Percentage) : null;

                    // Hide correct answers from student before submission
                    foreach (var question in dto.Questions)
                    {
                        question.Explanation = null;
                        foreach (var opt in question.Options)
                        {
                            opt.IsCorrect = null;
                        }
                    }
                }
            }

            return dtos;
        }

        public async Task<QuizDto?> GetQuizByIdAsync(int quizId, int? studentId = null)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions.OrderBy(qq => qq.OrderIndex))
                    .ThenInclude(qq => qq.Options)
                .Include(q => q.Attempts)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null) return null;

            var dto = _mapper.Map<QuizDto>(quiz);

            if (studentId.HasValue)
            {
                var studentAttempts = quiz.Attempts.Where(a => a.StudentId == studentId.Value).ToList();
                dto.IsPassedByStudent = studentAttempts.Any(a => a.IsPassed);
                dto.BestScorePercentage = studentAttempts.Any() ? studentAttempts.Max(a => a.Percentage) : null;

                foreach (var question in dto.Questions)
                {
                    question.Explanation = null;
                    foreach (var opt in question.Options)
                    {
                        opt.IsCorrect = null;
                    }
                }
            }

            return dto;
        }

        public async Task<QuizDto> CreateQuizAsync(int courseId, CreateQuizDto dto)
        {
            var quiz = _mapper.Map<Quiz>(dto);
            quiz.CourseId = courseId;

            await _context.Quizzes.AddAsync(quiz);
            await _context.SaveChangesAsync();

            return (await GetQuizByIdAsync(quiz.Id))!;
        }

        public async Task<bool> UpdateQuizAsync(int quizId, CreateQuizDto dto)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(qq => qq.Options)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null) return false;

            quiz.Title = dto.Title;
            quiz.Description = dto.Description;
            quiz.PassingScorePercentage = dto.PassingScorePercentage;
            quiz.TimeLimitMinutes = dto.TimeLimitMinutes;
            quiz.SectionId = dto.SectionId;
            if (dto.OrderIndex > 0) quiz.OrderIndex = dto.OrderIndex;

            _context.QuizQuestions.RemoveRange(quiz.Questions);
            quiz.Questions = _mapper.Map<List<QuizQuestion>>(dto.Questions);

            _context.Quizzes.Update(quiz);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteQuizAsync(int quizId)
        {
            var quiz = await _context.Quizzes.FindAsync(quizId);
            if (quiz == null) return false;

            _context.Quizzes.Remove(quiz);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<QuizResultDto> SubmitQuizAttemptAsync(int quizId, int studentId, SubmitQuizDto dto)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(qq => qq.Options)
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (quiz == null) throw new KeyNotFoundException("Quiz not found");

            int totalPoints = quiz.Questions.Sum(q => q.Points);
            int studentScore = 0;

            var attempt = new QuizAttempt
            {
                QuizId = quizId,
                StudentId = studentId,
                AttemptDate = DateTime.UtcNow,
                TotalPoints = totalPoints
            };

            foreach (var question in quiz.Questions)
            {
                var submittedAnswer = dto.Answers.FirstOrDefault(a => a.QuestionId == question.Id);
                bool isCorrect = false;
                var correctOption = question.Options.FirstOrDefault(o => o.IsCorrect);

                if (question.QuestionType == "MCQ" || question.QuestionType == "TrueFalse")
                {
                    if (submittedAnswer?.SelectedOptionId.HasValue == true)
                    {
                        var chosenOption = question.Options.FirstOrDefault(o => o.Id == submittedAnswer.SelectedOptionId.Value);
                        if (chosenOption != null && chosenOption.IsCorrect)
                        {
                            isCorrect = true;
                            studentScore += question.Points;
                        }
                    }
                }
                else if (question.QuestionType == "ShortAnswer")
                {
                    if (!string.IsNullOrWhiteSpace(submittedAnswer?.AnswerText) && correctOption != null)
                    {
                        if (string.Equals(submittedAnswer.AnswerText.Trim(), correctOption.OptionText.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            isCorrect = true;
                            studentScore += question.Points;
                        }
                    }
                }

                attempt.Answers.Add(new QuizAnswer
                {
                    QuestionId = question.Id,
                    SelectedOptionId = submittedAnswer?.SelectedOptionId,
                    AnswerText = submittedAnswer?.AnswerText,
                    IsCorrect = isCorrect
                });
            }

            attempt.Score = studentScore;
            attempt.Percentage = totalPoints > 0 ? Math.Round((decimal)studentScore / totalPoints * 100, 2) : 100;
            attempt.IsPassed = attempt.Percentage >= quiz.PassingScorePercentage;

            await _context.QuizAttempts.AddAsync(attempt);
            await _context.SaveChangesAsync();

            // Use AutoMapper to map attempt to QuizResultDto
            return _mapper.Map<QuizResultDto>(attempt);
        }

        public async Task<IEnumerable<QuizResultDto>> GetStudentAttemptsAsync(int quizId, int studentId)
        {
            var attempts = await _context.QuizAttempts
                .Include(a => a.Answers)
                    .ThenInclude(ans => ans.Question)
                        .ThenInclude(q => q.Options)
                .Where(a => a.QuizId == quizId && a.StudentId == studentId)
                .OrderByDescending(a => a.AttemptDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<QuizResultDto>>(attempts);
        }
    }
}
