using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GenAlpha.Business.DTOs;
using GenAlpha.Business.Interfaces;
using System;
using System.Threading.Tasks;

namespace GenAlpha.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CouponsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public CouponsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("validate")]
        [AllowAnonymous]
        public async Task<IActionResult> ValidateCoupon([FromBody] ValidateCouponRequestDto dto)
        {
            var result = await _paymentService.ValidateCouponAsync(dto.Code, dto.CourseId);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> CreateCoupon([FromBody] CreateCouponDto dto)
        {
            try
            {
                var coupon = await _paymentService.CreateCouponAsync(dto);
                return Ok(coupon);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Instructor")]
        public async Task<IActionResult> GetCoupons([FromQuery] int? courseId)
        {
            var coupons = await _paymentService.GetCouponsAsync(courseId);
            return Ok(coupons);
        }
    }
}
