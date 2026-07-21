using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MiniLMS.Business.DTOs;
using MiniLMS.Business.Interfaces;

namespace MiniLMS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")] 
    public class AdminCoursesController : ControllerBase
    {
        private readonly ICourseService _courseService;

        public AdminCoursesController(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var courses = await _courseService.GetAllCoursesAsync();
            return Ok(courses);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var course = await _courseService.GetCourseByIdAsync(id);
            if (course == null) return NotFound();
            return Ok(course);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCourseDto dto)
        {
            var course = await _courseService.CreateCourseAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = course.Id }, course);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateCourseDto dto)
        {
            var success = await _courseService.UpdateCourseAsync(id, dto);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _courseService.DeleteCourseAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }

        [HttpPost("{courseId}/lessons")]
        public async Task<IActionResult> AddLesson(int courseId, [FromBody] CreateLessonDto dto)
        {
            var lesson = await _courseService.AddLessonToCourseAsync(courseId, dto);
            if (lesson == null) return NotFound(new { message = "Course not found" });
            return Ok(lesson);
        }

        [HttpPost("{courseId}/lessons/reorder")]
        public async Task<IActionResult> ReorderLessons(int courseId, [FromBody] List<int> lessonIds)
        {
            var success = await _courseService.ReorderLessonsAsync(courseId, lessonIds);
            if (!success) return BadRequest(new { message = "Failed to reorder lessons" });
            return Ok(new { message = "Lessons reordered successfully" });
        }

        [HttpPost("{courseId}/publish")]
        public async Task<IActionResult> Publish(int courseId)
        {
            try
            {
                var success = await _courseService.PublishCourseAsync(courseId);
                if (!success) return NotFound();
                return Ok(new { message = "Course published successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message }); 
            }
        }
    }
}
