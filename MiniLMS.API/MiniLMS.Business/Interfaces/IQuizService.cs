using GenAlpha.Business.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenAlpha.Business.Interfaces
{
    public interface IQuizService
    {
        Task<IEnumerable<QuizDto>> GetCourseQuizzesAsync(int courseId, int? studentId = null);
        Task<QuizDto?> GetQuizByIdAsync(int quizId, int? studentId = null);
        Task<QuizDto> CreateQuizAsync(int courseId, CreateQuizDto dto);
        Task<bool> UpdateQuizAsync(int quizId, CreateQuizDto dto);
        Task<bool> DeleteQuizAsync(int quizId);
        Task<QuizResultDto> SubmitQuizAttemptAsync(int quizId, int studentId, SubmitQuizDto dto);
        Task<IEnumerable<QuizResultDto>> GetStudentAttemptsAsync(int quizId, int studentId);
    }
}
