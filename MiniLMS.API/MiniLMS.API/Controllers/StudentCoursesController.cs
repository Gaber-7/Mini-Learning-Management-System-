using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MiniLMS.Business.Interfaces;
using System.Security.Claims;

namespace MiniLMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Student")]
    public class StudentCoursesController : ControllerBase
    {
        private readonly IStudentService _studentService;

        public StudentCoursesController(IStudentService studentService)
        {
            _studentService = studentService;
        }
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(userIdClaim!.Value);
        }

        [HttpGet("available")]
        public async Task<IActionResult> GetAvailable([FromQuery] string? search, [FromQuery] string? category)
        {
            var courses = await _studentService.GetAvailableCoursesForStudentsAsync(search, category);
            return Ok(courses);
        }

        [HttpPost("enroll/{courseId}")]
        public async Task<IActionResult> Enroll(int courseId)
        {
            try
            {
                var studentId = GetCurrentUserId();
                var result = await _studentService.EnrollInCourseAsync(studentId, courseId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("my-courses")]
        public async Task<IActionResult> GetMyCourses()
        {
            var studentId = GetCurrentUserId();
            var result = await _studentService.GetMyEnrollmentsAsync(studentId);
            return Ok(result);
        }

        [HttpGet("details/{courseId}")]
        public async Task<IActionResult> GetCourseDetails(int courseId)
        {
            var studentId = GetCurrentUserId();
            var details = await _studentService.GetCourseDetailsForStudentAsync(studentId, courseId);
            if (details == null) return NotFound();
            return Ok(details);
        }

        [HttpPost("enrollments/{enrollmentId}/complete-lesson/{lessonId}")]
        public async Task<IActionResult> CompleteLesson(int enrollmentId, int lessonId)
        {
            var studentId = GetCurrentUserId();
            var success = await _studentService.CompleteLessonAsync(studentId, enrollmentId, lessonId);
            if (!success) return BadRequest(new { message = "Could not update lesson progress." });
            return Ok(new { message = "Progress updated successfully." });
        }
    }
}
