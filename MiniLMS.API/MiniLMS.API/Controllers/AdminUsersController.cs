using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MiniLMS.Business.Interfaces;

namespace MiniLMS.API.Controllers
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
    }
}
