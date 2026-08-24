using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MiniLMS.Business.DTOs;
using MiniLMS.Business.Interfaces;
using System.Security.Claims;

namespace MiniLMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class QnAController : ControllerBase
    {
        private readonly IQnAService _qnaService;

        public QnAController(IQnAService qnaService)
        {
            _qnaService = qnaService;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(claim!.Value);
        }

        private string GetCurrentUserRole()
        {
            var claim = User.FindFirst(ClaimTypes.Role);
            return claim?.Value ?? "Student";
        }

        [HttpGet("lessons/{lessonId}")]
        public async Task<IActionResult> GetLessonQuestions(int lessonId)
        {
            var questions = await _qnaService.GetLessonQuestionsAsync(lessonId);
            return Ok(questions);
        }

        [HttpGet("questions/{questionId}")]
        public async Task<IActionResult> GetQuestionById(int questionId)
        {
            var question = await _qnaService.GetQuestionByIdAsync(questionId);
            if (question == null) return NotFound(new { message = "Question not found" });
            return Ok(question);
        }

        [HttpPost("lessons/{lessonId}")]
        public async Task<IActionResult> AskQuestion(int lessonId, [FromBody] CreateLessonQuestionDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var studentId = GetCurrentUserId();
            var question = await _qnaService.AskQuestionAsync(lessonId, studentId, dto);
            return Ok(question);
        }

        [HttpPost("questions/{questionId}/replies")]
        public async Task<IActionResult> AddReply(int questionId, [FromBody] CreateLessonReplyDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            var userRole = GetCurrentUserRole();

            try
            {
                var reply = await _qnaService.AddReplyAsync(questionId, userId, userRole, dto);
                return Ok(reply);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("questions/{questionId}/toggle-resolved")]
        public async Task<IActionResult> ToggleResolved(int questionId)
        {
            var userId = GetCurrentUserId();
            var success = await _qnaService.ToggleResolvedAsync(questionId, userId);
            if (!success) return NotFound(new { message = "Question not found" });
            return Ok(new { message = "Question status updated" });
        }

        [HttpPost("replies/{replyId}/accept-answer")]
        public async Task<IActionResult> MarkAcceptedAnswer(int replyId)
        {
            var userId = GetCurrentUserId();
            var success = await _qnaService.MarkAcceptedAnswerAsync(replyId, userId);
            if (!success) return NotFound(new { message = "Reply not found" });
            return Ok(new { message = "Marked as accepted best answer" });
        }

        [HttpPost("questions/{questionId}/upvote")]
        public async Task<IActionResult> UpvoteQuestion(int questionId)
        {
            var count = await _qnaService.UpvoteQuestionAsync(questionId);
            return Ok(new { upvotesCount = count });
        }

        [HttpPost("replies/{replyId}/upvote")]
        public async Task<IActionResult> UpvoteReply(int replyId)
        {
            var count = await _qnaService.UpvoteReplyAsync(replyId);
            return Ok(new { upvotesCount = count });
        }
    }
}
