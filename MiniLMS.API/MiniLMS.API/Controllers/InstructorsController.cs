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
    public class InstructorsController : ControllerBase
    {
        private readonly IInstructorService _instructorService;
        private readonly IAdminUserService _adminUserService;

        public InstructorsController(IInstructorService instructorService, IAdminUserService adminUserService)
        {
            _instructorService = instructorService;
            _adminUserService = adminUserService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var instructors = await _adminUserService.GetAllInstructorsAsync();
            return Ok(instructors);
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(claim!.Value);
        }

        [HttpGet("profile")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GetProfile()
        {
            var instructorId = GetCurrentUserId();
            var profile = await _instructorService.GetProfileAsync(instructorId);
            if (profile == null) return NotFound(new { message = "Instructor profile not found." });
            return Ok(profile);
        }

        [HttpPut("profile")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateInstructorProfileDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var instructorId = GetCurrentUserId();
            var success = await _instructorService.UpdateProfileAsync(instructorId, dto);
            if (!success) return NotFound(new { message = "Instructor profile not found." });
            return Ok(new { message = "Profile updated successfully." });
        }

        [HttpGet("{id}/public")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicProfile(int id)
        {
            var profile = await _instructorService.GetPublicProfileAsync(id);
            if (profile == null) return NotFound(new { message = "Instructor not found." });
            return Ok(profile);
        }
    }
}
