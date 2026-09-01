using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GenAlpha.Business.DTOs;
using GenAlpha.Business.Interfaces;
using System.Security.Claims;

namespace GenAlpha.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class QuizzesController : ControllerBase
    {
        private readonly IQuizService _quizService;

        public QuizzesController(IQuizService quizService)
        {
            _quizService = quizService;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(claim!.Value);
        }

        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetCourseQuizzes(int courseId)
        {
            var userId = GetCurrentUserId();
            var quizzes = await _quizService.GetCourseQuizzesAsync(courseId, userId);
            return Ok(quizzes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuizById(int id)
        {
            var userId = GetCurrentUserId();
            var quiz = await _quizService.GetQuizByIdAsync(id, userId);
            if (quiz == null) return NotFound(new { message = "Quiz not found" });
            return Ok(quiz);
        }

        [HttpPost("course/{courseId}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> CreateQuiz(int courseId, [FromBody] CreateQuizDto dto)
        {
            var quiz = await _quizService.CreateQuizAsync(courseId, dto);
            return CreatedAtAction(nameof(GetQuizById), new { id = quiz.Id }, quiz);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> UpdateQuiz(int id, [FromBody] CreateQuizDto dto)
        {
            var success = await _quizService.UpdateQuizAsync(id, dto);
            if (!success) return NotFound(new { message = "Quiz not found" });
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            var success = await _quizService.DeleteQuizAsync(id);
            if (!success) return NotFound(new { message = "Quiz not found" });
            return NoContent();
        }

        [HttpPost("{id}/submit")]
        public async Task<IActionResult> SubmitQuizAttempt(int id, [FromBody] SubmitQuizDto dto)
        {
            try
            {
                var studentId = GetCurrentUserId();
                var result = await _quizService.SubmitQuizAttemptAsync(id, studentId, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("{id}/attempts")]
        public async Task<IActionResult> GetMyAttempts(int id)
        {
            var studentId = GetCurrentUserId();
            var attempts = await _quizService.GetStudentAttemptsAsync(id, studentId);
            return Ok(attempts);
        }
    }
}
