using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GenAlpha.Business.DTOs;
using GenAlpha.Business.Interfaces;
using System.Threading.Tasks;

namespace GenAlpha.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AiTutorController : ControllerBase
    {
        private readonly IAiTutorService _aiTutorService;

        public AiTutorController(IAiTutorService aiTutorService)
        {
            _aiTutorService = aiTutorService;
        }

        [HttpPost("explain")]
        [AllowAnonymous]
        public async Task<IActionResult> Explain([FromBody] AiExplainRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Prompt))
            {
                return BadRequest(new { message = "Prompt cannot be empty." });
            }

            var result = await _aiTutorService.ExplainConceptAsync(request);
            return Ok(result);
        }

        [HttpPost("summarize")]
        [AllowAnonymous]
        public async Task<IActionResult> Summarize([FromBody] AiSummarizeRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.LessonTitle))
            {
                return BadRequest(new { message = "Lesson title cannot be empty." });
            }

            var result = await _aiTutorService.SummarizeLessonAsync(request);
            return Ok(result);
        }

        [HttpPost("practice-questions")]
        [AllowAnonymous]
        public async Task<IActionResult> PracticeQuestions([FromBody] AiPracticeQuestionsRequestDto request)
        {
            var result = await _aiTutorService.GeneratePracticeQuestionsAsync(request);
            return Ok(result);
        }
    }
}
