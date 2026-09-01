using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GenAlpha.Business.DTOs;
using GenAlpha.Business.Interfaces;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GenAlpha.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GamificationController : ControllerBase
    {
        private readonly IGamificationService _gamificationService;

        public GamificationController(IGamificationService gamificationService)
        {
            _gamificationService = gamificationService;
        }

        [HttpGet("profile/{studentId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProfile(int studentId)
        {
            try
            {
                var profile = await _gamificationService.GetStudentProfileAsync(studentId);
                return Ok(profile);
            }
            catch (System.Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("my-profile")]
        [Authorize]
        public async Task<IActionResult> GetMyProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int studentId))
            {
                return Unauthorized(new { message = "Invalid token." });
            }

            var profile = await _gamificationService.GetStudentProfileAsync(studentId);
            return Ok(profile);
        }

        [HttpGet("badges/{studentId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBadges(int studentId)
        {
            var badges = await _gamificationService.GetAllBadgesWithStudentStatusAsync(studentId);
            return Ok(badges);
        }

        [HttpGet("leaderboard")]
        [AllowAnonymous]
        public async Task<IActionResult> GetLeaderboard([FromQuery] int top = 10)
        {
            var leaderboard = await _gamificationService.GetLeaderboardAsync(top);
            return Ok(leaderboard);
        }

        [HttpPost("award-xp")]
        [Authorize]
        public async Task<IActionResult> AwardXP([FromBody] AddXpRequestDto request)
        {
            var studentId = request.StudentId;
            if (studentId == 0)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int uid))
                {
                    studentId = uid;
                }
            }

            var updated = await _gamificationService.AwardXPAsync(studentId, request.Amount, request.Reason);
            return Ok(updated);
        }
    }
}
