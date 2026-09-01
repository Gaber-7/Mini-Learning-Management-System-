using System;
using System.Collections.Generic;

namespace GenAlpha.Business.DTOs
{
    public class ValidateCouponRequestDto
    {
        public string Code { get; set; } = string.Empty;
        public int CourseId { get; set; }
    }

    public class CouponResultDto
    {
        public bool IsValid { get; set; }
        public string Code { get; set; } = string.Empty;
        public decimal OriginalPrice { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal FinalPrice { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class CreateCouponDto
    {
        public string Code { get; set; } = string.Empty;
        public decimal DiscountPercentage { get; set; }
        public decimal DiscountAmount { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int MaxUsageCount { get; set; } = 100;
        public int? CourseId { get; set; }
    }

    public class CreatePaymentOrderDto
    {
        public int CourseId { get; set; }
        public string? CouponCode { get; set; }
        public string PaymentMethod { get; set; } = "PayPal";
    }

    public class PaymentOrderResultDto
    {
        public string OrderId { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public decimal OriginalPrice { get; set; }
        public decimal FinalAmount { get; set; }
        public string Currency { get; set; } = "USD";
        public string ApprovalUrl { get; set; } = string.Empty;
        public string Status { get; set; } = "Created";
    }

    public class CapturePaymentDto
    {
        public string OrderId { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string? CouponCode { get; set; }
        public string? TransactionId { get; set; }
        public string PaymentMethod { get; set; } = "PayPal";
    }

    public class PaymentResultDto
    {
        public bool Success { get; set; }
        public int PaymentId { get; set; }
        public int CourseId { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public decimal AmountPaid { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool Enrolled { get; set; } = true;
    }

    public class InstructorWalletDto
    {
        public int InstructorId { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public decimal TotalEarnings { get; set; }
        public decimal AvailableBalance { get; set; }
        public decimal WithdrawnAmount { get; set; }
        public decimal CommissionPercentage { get; set; }
        public DateTime LastUpdated { get; set; }
        public List<CoursePaymentSummaryDto> RecentPayments { get; set; } = new List<CoursePaymentSummaryDto>();
    }

    public class CoursePaymentSummaryDto
    {
        public int PaymentId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal InstructorShare { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; } = "Completed";
    }

    public class PayoutRequestDto
    {
        public decimal Amount { get; set; }
        public string PayoutAccount { get; set; } = string.Empty; // PayPal email
        public string? Note { get; set; }
    }
}
