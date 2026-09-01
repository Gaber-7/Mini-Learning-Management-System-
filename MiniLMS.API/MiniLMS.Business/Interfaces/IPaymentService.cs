using GenAlpha.Business.DTOs;
using GenAlpha.Data.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GenAlpha.Business.Interfaces
{
    public interface IPaymentService
    {
        Task<CouponResultDto> ValidateCouponAsync(string code, int courseId);
        Task<Coupon> CreateCouponAsync(CreateCouponDto dto);
        Task<List<Coupon>> GetCouponsAsync(int? courseId = null);
        Task<PaymentOrderResultDto> CreatePaymentOrderAsync(int studentId, CreatePaymentOrderDto dto);
        Task<PaymentResultDto> CapturePaymentAsync(int studentId, CapturePaymentDto dto);
        Task<InstructorWalletDto> GetInstructorWalletAsync(int instructorId);
        Task<bool> RequestPayoutAsync(int instructorId, PayoutRequestDto dto);
    }
}
