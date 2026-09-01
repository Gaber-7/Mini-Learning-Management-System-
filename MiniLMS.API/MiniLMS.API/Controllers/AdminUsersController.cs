using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using GenAlpha.Business.DTOs;
using GenAlpha.Business.Interfaces;

namespace GenAlpha.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminUsersController : ControllerBase
    {
        private readonly IAdminUserService _adminUserService;

        public AdminUsersController(IAdminUserService adminUserService)
        {
            _adminUserService = adminUserService;
        }

        // ==================== STUDENTS ====================

        [HttpGet("students")]
        public async Task<IActionResult> GetAllStudents()
        {
            var students = await _adminUserService.GetAllStudentsAsync();
            return Ok(students);
        }

        [HttpGet("students/{id}")]
        public async Task<IActionResult> GetStudentById(int id)
        {
            var student = await _adminUserService.GetStudentByIdAsync(id);
            if (student == null) return NotFound(new { message = "Student not found" });
            return Ok(student);
        }

        [HttpPost("students")]
        public async Task<IActionResult> CreateStudent([FromBody] CreateStudentDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var created = await _adminUserService.CreateStudentAsync(dto);
                return CreatedAtAction(nameof(GetStudentById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("students/{id}")]
        public async Task<IActionResult> UpdateStudent(int id, [FromBody] UpdateStudentDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var success = await _adminUserService.UpdateStudentAsync(id, dto);
            if (!success) return NotFound(new { message = "Student not found" });
            return Ok(new { message = "Student updated successfully" });
        }

        [HttpDelete("students/{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var success = await _adminUserService.DeleteStudentAsync(id);
            if (!success) return NotFound(new { message = "Student not found" });
            return Ok(new { message = "Student deleted successfully" });
        }

        // ==================== INSTRUCTORS ====================

        [HttpGet("instructors")]
        public async Task<IActionResult> GetAllInstructors()
        {
            var instructors = await _adminUserService.GetAllInstructorsAsync();
            return Ok(instructors);
        }

        [HttpGet("instructors/{id}")]
        public async Task<IActionResult> GetInstructorById(int id)
        {
            var instructor = await _adminUserService.GetInstructorByIdAsync(id);
            if (instructor == null) return NotFound(new { message = "Instructor not found" });
            return Ok(instructor);
        }

        [HttpPost("instructors")]
        public async Task<IActionResult> CreateInstructor([FromBody] CreateInstructorDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var created = await _adminUserService.CreateInstructorAsync(dto);
                return CreatedAtAction(nameof(GetInstructorById), new { id = created.Id }, created);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("instructors/{id}")]
        public async Task<IActionResult> UpdateInstructor(int id, [FromBody] UpdateInstructorAdminDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var success = await _adminUserService.UpdateInstructorAsync(id, dto);
            if (!success) return NotFound(new { message = "Instructor not found" });
            return Ok(new { message = "Instructor updated successfully" });
        }

        [HttpDelete("instructors/{id}")]
        public async Task<IActionResult> DeleteInstructor(int id)
        {
            var success = await _adminUserService.DeleteInstructorAsync(id);
            if (!success) return NotFound(new { message = "Instructor not found" });
            return Ok(new { message = "Instructor deleted successfully" });
        }

        // ==================== REVIEWS ====================

        [HttpGet("reviews")]
        public async Task<IActionResult> GetAllReviews()
        {
            var reviews = await _adminUserService.GetAllReviewsAsync();
            return Ok(reviews);
        }

        [HttpPost("reviews/{id}/toggle-approval")]
        public async Task<IActionResult> ToggleReviewApproval(int id)
        {
            var success = await _adminUserService.ToggleReviewApprovalAsync(id);
            if (!success) return NotFound(new { message = "Review not found" });
            return Ok(new { message = "Review status updated successfully" });
        }

        [HttpDelete("reviews/{id}")]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var success = await _adminUserService.DeleteReviewAsync(id);
            if (!success) return NotFound(new { message = "Review not found" });
            return Ok(new { message = "Review deleted successfully" });
        }
    }
}
