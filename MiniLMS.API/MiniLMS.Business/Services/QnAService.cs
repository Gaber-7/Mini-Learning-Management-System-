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
    public class QnAService : IQnAService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public QnAService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<LessonQuestionDto>> GetLessonQuestionsAsync(int lessonId)
        {
            var questions = await _context.LessonQuestions
                .Include(q => q.Student)
                .Include(q => q.Lesson)
                .Include(q => q.Replies)
                    .ThenInclude(r => r.User)
                        .ThenInclude(u => u.Instructor)
                .Include(q => q.Replies)
                    .ThenInclude(r => r.User)
                        .ThenInclude(u => u.Student)
                .Where(q => q.LessonId == lessonId)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<LessonQuestionDto>>(questions);
        }

        public async Task<LessonQuestionDto?> GetQuestionByIdAsync(int questionId)
        {
            var q = await _context.LessonQuestions
                .Include(x => x.Student)
                .Include(x => x.Lesson)
                .Include(x => x.Replies.OrderBy(r => r.CreatedAt))
                    .ThenInclude(r => r.User)
                        .ThenInclude(u => u.Instructor)
                .Include(x => x.Replies.OrderBy(r => r.CreatedAt))
                    .ThenInclude(r => r.User)
                        .ThenInclude(u => u.Student)
                .FirstOrDefaultAsync(x => x.Id == questionId);

            return q == null ? null : _mapper.Map<LessonQuestionDto>(q);
        }

        public async Task<LessonQuestionDto> AskQuestionAsync(int lessonId, int studentId, CreateLessonQuestionDto dto)
        {
            var question = _mapper.Map<LessonQuestion>(dto);
            question.LessonId = lessonId;
            question.StudentId = studentId;
            question.CreatedAt = DateTime.UtcNow;

            await _context.LessonQuestions.AddAsync(question);
            await _context.SaveChangesAsync();

            return (await GetQuestionByIdAsync(question.Id))!;
        }

        public async Task<LessonReplyDto> AddReplyAsync(int questionId, int userId, string userRole, CreateLessonReplyDto dto)
        {
            var question = await _context.LessonQuestions.FindAsync(questionId);
            if (question == null) throw new KeyNotFoundException("Question not found");

            var reply = _mapper.Map<LessonReply>(dto);
            reply.QuestionId = questionId;
            reply.UserId = userId;
            reply.CreatedAt = DateTime.UtcNow;
            reply.IsInstructorReply = userRole == "Instructor" || userRole == "Admin";

            await _context.LessonReplies.AddAsync(reply);
            await _context.SaveChangesAsync();

            var user = await _context.Users
                .Include(u => u.Student)
                .Include(u => u.Instructor)
                .FirstOrDefaultAsync(u => u.Id == userId);

            reply.User = user!;

            return _mapper.Map<LessonReplyDto>(reply);
        }

        public async Task<bool> ToggleResolvedAsync(int questionId, int userId)
        {
            var q = await _context.LessonQuestions.FindAsync(questionId);
            if (q == null) return false;

            q.IsResolved = !q.IsResolved;
            _context.LessonQuestions.Update(q);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> MarkAcceptedAnswerAsync(int replyId, int userId)
        {
            var reply = await _context.LessonReplies
                .Include(r => r.Question)
                .FirstOrDefaultAsync(r => r.Id == replyId);

            if (reply == null) return false;

            var allReplies = await _context.LessonReplies.Where(r => r.QuestionId == reply.QuestionId).ToListAsync();
            foreach (var r in allReplies)
            {
                r.IsAcceptedAnswer = (r.Id == replyId);
            }

            reply.Question.IsResolved = true;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> UpvoteQuestionAsync(int questionId)
        {
            var q = await _context.LessonQuestions.FindAsync(questionId);
            if (q == null) return 0;

            q.UpvotesCount += 1;
            await _context.SaveChangesAsync();
            return q.UpvotesCount;
        }

        public async Task<int> UpvoteReplyAsync(int replyId)
        {
            var r = await _context.LessonReplies.FindAsync(replyId);
            if (r == null) return 0;

            r.UpvotesCount += 1;
            await _context.SaveChangesAsync();
            return r.UpvotesCount;
        }
    }
}
