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
    [Authorize(Roles = "Instructor")]
    public class InstructorCoursesController : ControllerBase
    {
        private readonly IInstructorService _instructorService;

        public InstructorCoursesController(IInstructorService instructorService)
        {
            _instructorService = instructorService;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(claim!.Value);
        }

        // ==================== Courses ====================

        [HttpGet]
        public async Task<IActionResult> GetMyCourses()
        {
            var instructorId = GetCurrentUserId();
            var courses = await _instructorService.GetMyCoursesAsync(instructorId);
            return Ok(courses);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var instructorId = GetCurrentUserId();
            var course = await _instructorService.GetMyCourseByIdAsync(instructorId, id);
            if (course == null) return NotFound(new { message = "Course not found or unauthorized." });
            return Ok(course);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCourseDto dto)
        {
            var instructorId = GetCurrentUserId();
            var course = await _instructorService.CreateCourseAsync(instructorId, dto);
            return CreatedAtAction(nameof(GetById), new { id = course.Id }, course);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateCourseDto dto)
        {
            var instructorId = GetCurrentUserId();
            var success = await _instructorService.UpdateCourseAsync(instructorId, id, dto);
            if (!success) return NotFound(new { message = "Course not found or unauthorized." });
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var instructorId = GetCurrentUserId();
            var success = await _instructorService.DeleteCourseAsync(instructorId, id);
            if (!success) return NotFound(new { message = "Course not found or unauthorized." });
            return NoContent();
        }

        [HttpPost("{id}/submit-review")]
        public async Task<IActionResult> SubmitForReview(int id)
        {
            try
            {
                var instructorId = GetCurrentUserId();
                var success = await _instructorService.SubmitForReviewAsync(instructorId, id);
                if (!success) return NotFound(new { message = "Course not found." });
                return Ok(new { message = "Course submitted for review successfully. An admin will review it shortly." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ==================== Enrolled Students ====================

        [HttpGet("students")]
        public async Task<IActionResult> GetStudents()
        {
            var instructorId = GetCurrentUserId();
            var students = await _instructorService.GetMyStudentsAsync(instructorId);
            return Ok(students);
        }

        // ==================== Sections ====================

        [HttpPost("{courseId}/sections")]
        public async Task<IActionResult> AddSection(int courseId, [FromBody] CreateSectionDto dto)
        {
            var instructorId = GetCurrentUserId();
            var section = await _instructorService.AddSectionAsync(instructorId, courseId, dto);
            if (section == null) return NotFound(new { message = "Course not found or unauthorized." });
            return Ok(section);
        }

        [HttpPut("sections/{sectionId}")]
        public async Task<IActionResult> UpdateSection(int sectionId, [FromBody] CreateSectionDto dto)
        {
            var instructorId = GetCurrentUserId();
            var success = await _instructorService.UpdateSectionAsync(instructorId, sectionId, dto);
            if (!success) return NotFound(new { message = "Section not found or unauthorized." });
            return NoContent();
        }

        [HttpDelete("sections/{sectionId}")]
        public async Task<IActionResult> DeleteSection(int sectionId)
        {
            var instructorId = GetCurrentUserId();
            var success = await _instructorService.DeleteSectionAsync(instructorId, sectionId);
            if (!success) return NotFound(new { message = "Section not found or unauthorized." });
            return NoContent();
        }

        // ==================== Lessons ====================

        [HttpPost("sections/{sectionId}/lessons")]
        public async Task<IActionResult> AddLessonToSection(int sectionId, [FromBody] CreateLessonDto dto)
        {
            var instructorId = GetCurrentUserId();
            var lesson = await _instructorService.AddLessonToSectionAsync(instructorId, sectionId, dto);
            if (lesson == null) return NotFound(new { message = "Section not found or unauthorized." });
            return Ok(lesson);
        }

        [HttpPut("lessons/{lessonId}")]
        public async Task<IActionResult> UpdateLesson(int lessonId, [FromBody] CreateLessonDto dto)
        {
            var instructorId = GetCurrentUserId();
            var success = await _instructorService.UpdateLessonAsync(instructorId, lessonId, dto);
            if (!success) return NotFound(new { message = "Lesson not found or unauthorized." });
            return NoContent();
        }

        [HttpDelete("lessons/{lessonId}")]
        public async Task<IActionResult> DeleteLesson(int lessonId)
        {
            var instructorId = GetCurrentUserId();
            var success = await _instructorService.DeleteLessonAsync(instructorId, lessonId);
            if (!success) return NotFound(new { message = "Lesson not found or unauthorized." });
            return NoContent();
        }
    }
}
