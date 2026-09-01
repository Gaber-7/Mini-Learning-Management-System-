using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GenAlpha.Business.DTOs;
using GenAlpha.Business.Interfaces;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GenAlpha.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CertificatesController : ControllerBase
    {
        private readonly ICertificateService _certificateService;

        public CertificatesController(ICertificateService certificateService)
        {
            _certificateService = certificateService;
        }

        public class IssueCertificateRequest
        {
            public int StudentId { get; set; }
            public int CourseId { get; set; }
            public decimal? FinalScore { get; set; } = 100;
        }

        [HttpPost("issue")]
        [Authorize]
        public async Task<IActionResult> IssueCertificate([FromBody] IssueCertificateRequest request)
        {
            try
            {
                // If studentId not specified, try to read from token claims
                var studentId = request.StudentId;
                if (studentId == 0)
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (int.TryParse(userIdClaim, out int uid))
                    {
                        studentId = uid;
                    }
                }

                var cert = await _certificateService.IssueCertificateAsync(studentId, request.CourseId, request.FinalScore);
                return Ok(cert);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while issuing certificate.", error = ex.Message });
            }
        }

        [HttpGet("verify/{code}")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyCertificate(string code)
        {
            var result = await _certificateService.VerifyCertificateAsync(code);
            if (result == null || !result.IsValid)
            {
                return NotFound(new { message = "Certificate not found or invalid.", isValid = false });
            }

            return Ok(result);
        }

        [HttpGet("student/{studentId}")]
        [Authorize]
        public async Task<IActionResult> GetStudentCertificates(int studentId)
        {
            var certs = await _certificateService.GetStudentCertificatesAsync(studentId);
            return Ok(certs);
        }

        [HttpGet("course/{courseId}")]
        [Authorize]
        public async Task<IActionResult> GetCertificateForCourse(int courseId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int studentId))
            {
                return Unauthorized(new { message = "Invalid user token." });
            }

            var cert = await _certificateService.GetCertificateByStudentAndCourseAsync(studentId, courseId);
            if (cert == null)
            {
                return NotFound(new { message = "No certificate found for this course." });
            }

            return Ok(cert);
        }

        [HttpGet("download/{code}")]
        [AllowAnonymous]
        public async Task<IActionResult> DownloadCertificatePdf(string code)
        {
            var pdfBytes = await _certificateService.GenerateCertificatePdfAsync(code);
            if (pdfBytes == null || pdfBytes.Length == 0)
            {
                return NotFound(new { message = "Certificate not found or could not be generated." });
            }

            return File(pdfBytes, "application/pdf", $"Certificate_{code}.pdf");
        }
    }
}
