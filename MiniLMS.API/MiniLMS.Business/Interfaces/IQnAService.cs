using MiniLMS.Business.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniLMS.Business.Interfaces
{
    public interface IQnAService
    {
        Task<IEnumerable<LessonQuestionDto>> GetLessonQuestionsAsync(int lessonId);
        Task<LessonQuestionDto?> GetQuestionByIdAsync(int questionId);
        Task<LessonQuestionDto> AskQuestionAsync(int lessonId, int studentId, CreateLessonQuestionDto dto);
        Task<LessonReplyDto> AddReplyAsync(int questionId, int userId, string userRole, CreateLessonReplyDto dto);
        Task<bool> ToggleResolvedAsync(int questionId, int userId);
        Task<bool> MarkAcceptedAnswerAsync(int replyId, int userId);
        Task<int> UpvoteQuestionAsync(int questionId);
        Task<int> UpvoteReplyAsync(int replyId);
    }
}
