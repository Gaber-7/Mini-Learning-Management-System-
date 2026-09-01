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
    [Authorize]
    public class AssignmentsController : ControllerBase
    {
        private readonly IAssignmentService _assignmentService;

        public AssignmentsController(IAssignmentService assignmentService)
        {
            _assignmentService = assignmentService;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return int.Parse(claim!.Value);
        }

        [HttpGet("course/{courseId}")]
        public async Task<IActionResult> GetCourseAssignments(int courseId)
        {
            var userId = GetCurrentUserId();
            var assignments = await _assignmentService.GetCourseAssignmentsAsync(courseId, userId);
            return Ok(assignments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAssignmentById(int id)
        {
            var userId = GetCurrentUserId();
            var assignment = await _assignmentService.GetAssignmentByIdAsync(id, userId);
            if (assignment == null) return NotFound(new { message = "Assignment not found" });
            return Ok(assignment);
        }

        [HttpPost("course/{courseId}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> CreateAssignment(int courseId, [FromBody] CreateAssignmentDto dto)
        {
            var assignment = await _assignmentService.CreateAssignmentAsync(courseId, dto);
            return CreatedAtAction(nameof(GetAssignmentById), new { id = assignment.Id }, assignment);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> UpdateAssignment(int id, [FromBody] CreateAssignmentDto dto)
        {
            var success = await _assignmentService.UpdateAssignmentAsync(id, dto);
            if (!success) return NotFound(new { message = "Assignment not found" });
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> DeleteAssignment(int id)
        {
            var success = await _assignmentService.DeleteAssignmentAsync(id);
            if (!success) return NotFound(new { message = "Assignment not found" });
            return NoContent();
        }

        [HttpPost("{id}/submit")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> SubmitAssignment(int id, [FromBody] SubmitAssignmentDto dto)
        {
            var studentId = GetCurrentUserId();
            var submission = await _assignmentService.SubmitAssignmentAsync(id, studentId, dto);
            return Ok(submission);
        }

        [HttpGet("{id}/submissions")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> GetSubmissions(int id)
        {
            var submissions = await _assignmentService.GetAssignmentSubmissionsAsync(id);
            return Ok(submissions);
        }

        [HttpPost("submissions/{submissionId}/grade")]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> GradeSubmission(int submissionId, [FromBody] GradeAssignmentDto dto)
        {
            var success = await _assignmentService.GradeSubmissionAsync(submissionId, dto);
            if (!success) return NotFound(new { message = "Submission not found" });
            return Ok(new { message = "Assignment graded successfully." });
        }
    }
}
