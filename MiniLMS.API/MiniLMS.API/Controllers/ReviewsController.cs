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
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewsController(IReviewService reviewService)
        {
            _reviewService = reviewService;
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

        [HttpGet("course/{courseId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCourseReviews(int courseId)
        {
            var reviews = await _reviewService.GetCourseReviewsAsync(courseId);
            return Ok(reviews);
        }

        [HttpGet("course/{courseId}/summary")]
        [AllowAnonymous]
        public async Task<IActionResult> GetRatingSummary(int courseId)
        {
            var summary = await _reviewService.GetCourseRatingSummaryAsync(courseId);
            return Ok(summary);
        }

        [HttpPost("course/{courseId}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> AddOrUpdateReview(int courseId, [FromBody] CreateCourseReviewDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var studentId = GetCurrentUserId();
            var review = await _reviewService.AddOrUpdateReviewAsync(courseId, studentId, dto);
            return Ok(review);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var userId = GetCurrentUserId();
            var role = GetCurrentUserRole();
            var success = await _reviewService.DeleteReviewAsync(id, userId, role);
            if (!success) return NotFound(new { message = "Review not found or unauthorized" });
            return NoContent();
        }
    }
}
