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
        private readonly IInstructorService _instructorService;

        public AdminCoursesController(ICourseService courseService, IInstructorService instructorService)
        {
            _courseService = courseService;
            _instructorService = instructorService;
        }

        // ==================== Courses ====================

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

        // ==================== Course Approval Workflow ====================

        [HttpGet("pending-review")]
        public async Task<IActionResult> GetPendingReview()
        {
            var courses = await _instructorService.GetPendingReviewCoursesAsync();
            return Ok(courses);
        }

        [HttpPost("{courseId}/approve")]
        public async Task<IActionResult> ApproveCourse(int courseId)
        {
            var success = await _instructorService.ApproveCourseAsync(courseId);
            if (!success) return NotFound(new { message = "Course not found." });
            return Ok(new { message = "Course approved and published successfully." });
        }

        [HttpPost("{courseId}/reject")]
        public async Task<IActionResult> RejectCourse(int courseId, [FromBody] RejectCourseDto dto)
        {
            var success = await _instructorService.RejectCourseAsync(courseId, dto.Reason);
            if (!success) return NotFound(new { message = "Course not found." });
            return Ok(new { message = "Course rejected and feedback sent to instructor." });
        }

        // ==================== Sections ====================

        [HttpPost("{courseId}/sections")]
        public async Task<IActionResult> AddSection(int courseId, [FromBody] CreateSectionDto dto)
        {
            var section = await _courseService.AddSectionToCourseAsync(courseId, dto);
            if (section == null) return NotFound(new { message = "Course not found" });
            return Ok(section);
        }

        [HttpPut("sections/{sectionId}")]
        public async Task<IActionResult> UpdateSection(int sectionId, [FromBody] CreateSectionDto dto)
        {
            var success = await _courseService.UpdateSectionAsync(sectionId, dto);
            if (!success) return NotFound(new { message = "Section not found" });
            return NoContent();
        }

        [HttpDelete("sections/{sectionId}")]
        public async Task<IActionResult> DeleteSection(int sectionId)
        {
            var success = await _courseService.DeleteSectionAsync(sectionId);
            if (!success) return NotFound(new { message = "Section not found" });
            return NoContent();
        }

        [HttpPost("{courseId}/sections/reorder")]
        public async Task<IActionResult> ReorderSections(int courseId, [FromBody] List<int> sectionIds)
        {
            var success = await _courseService.ReorderSectionsAsync(courseId, sectionIds);
            if (!success) return BadRequest(new { message = "Failed to reorder sections" });
            return Ok(new { message = "Sections reordered successfully" });
        }

        // ==================== Lessons ====================

        [HttpPost("sections/{sectionId}/lessons")]
        public async Task<IActionResult> AddLessonToSection(int sectionId, [FromBody] CreateLessonDto dto)
        {
            var lesson = await _courseService.AddLessonToSectionAsync(sectionId, dto);
            if (lesson == null) return NotFound(new { message = "Section not found" });
            return Ok(lesson);
        }

        [HttpPost("{courseId}/lessons")]
        public async Task<IActionResult> AddLesson(int courseId, [FromBody] CreateLessonDto dto)
        {
            var lesson = await _courseService.AddLessonToCourseAsync(courseId, dto);
            if (lesson == null) return NotFound(new { message = "Course not found" });
            return Ok(lesson);
        }

        [HttpPut("lessons/{lessonId}")]
        public async Task<IActionResult> UpdateLesson(int lessonId, [FromBody] CreateLessonDto dto)
        {
            var success = await _courseService.UpdateLessonAsync(lessonId, dto);
            if (!success) return NotFound(new { message = "Lesson not found" });
            return NoContent();
        }

        [HttpDelete("lessons/{lessonId}")]
        public async Task<IActionResult> DeleteLesson(int lessonId)
        {
            var success = await _courseService.RemoveLessonAsync(lessonId);
            if (!success) return NotFound(new { message = "Lesson not found" });
            return NoContent();
        }

        [HttpPost("sections/{sectionId}/lessons/reorder")]
        public async Task<IActionResult> ReorderLessonsInSection(int sectionId, [FromBody] List<int> lessonIds)
        {
            var success = await _courseService.ReorderLessonsInSectionAsync(sectionId, lessonIds);
            if (!success) return BadRequest(new { message = "Failed to reorder lessons in section" });
            return Ok(new { message = "Lessons reordered successfully" });
        }

        [HttpPost("{courseId}/lessons/reorder")]
        public async Task<IActionResult> ReorderLessons(int courseId, [FromBody] List<int> lessonIds)
        {
            var success = await _courseService.ReorderLessonsAsync(courseId, lessonIds);
            if (!success) return BadRequest(new { message = "Failed to reorder lessons" });
            return Ok(new { message = "Lessons reordered successfully" });
        }
    }
}
