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
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(claim, out int id)) return id;
            return 0;
        }

        [HttpPost("create-order")]
        [Authorize]
        public async Task<IActionResult> CreateOrder([FromBody] CreatePaymentOrderDto dto)
        {
            var studentId = GetCurrentUserId();
            if (studentId == 0) return Unauthorized();

            try
            {
                var result = await _paymentService.CreatePaymentOrderAsync(studentId, dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("capture")]
        [Authorize]
        public async Task<IActionResult> CapturePayment([FromBody] CapturePaymentDto dto)
        {
            var studentId = GetCurrentUserId();
            if (studentId == 0) return Unauthorized();

            try
            {
                var result = await _paymentService.CapturePaymentAsync(studentId, dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("instructor/wallet")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> GetInstructorWallet()
        {
            var instructorId = GetCurrentUserId();
            if (instructorId == 0) return Unauthorized();

            try
            {
                var wallet = await _paymentService.GetInstructorWalletAsync(instructorId);
                return Ok(wallet);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("instructor/payout")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> RequestPayout([FromBody] PayoutRequestDto dto)
        {
            var instructorId = GetCurrentUserId();
            if (instructorId == 0) return Unauthorized();

            try
            {
                var success = await _paymentService.RequestPayoutAsync(instructorId, dto);
                return Ok(new { success, message = "Payout request submitted successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
