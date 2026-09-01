using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GenAlpha.Data.Models
{
    public class Coupon
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [Column(TypeName = "decimal(5, 2)")]
        public decimal DiscountPercentage { get; set; } = 0;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal DiscountAmount { get; set; } = 0;

        public DateTime? ExpiryDate { get; set; }

        public int MaxUsageCount { get; set; } = 100;

        public int CurrentUsageCount { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public int? CourseId { get; set; }
        public virtual Course? Course { get; set; }
    }

    public class CoursePayment
    {
        public int Id { get; set; }

        public int StudentId { get; set; }

        public int CourseId { get; set; }

        public int? CouponId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Amount { get; set; }

        [StringLength(50)]
        public string PaymentMethod { get; set; } = "PayPal";

        [StringLength(100)]
        public string? TransactionId { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Completed";

        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        public virtual Student Student { get; set; } = null!;
        public virtual Course Course { get; set; } = null!;
        public virtual Coupon? Coupon { get; set; }
    }

    public class InstructorWallet
    {
        [Key]
        [ForeignKey("Instructor")]
        public int InstructorId { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalEarnings { get; set; } = 0;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal AvailableBalance { get; set; } = 0;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal WithdrawnAmount { get; set; } = 0;

        [Column(TypeName = "decimal(5, 2)")]
        public decimal CommissionPercentage { get; set; } = 70.0m;

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        public virtual Instructor Instructor { get; set; } = null!;
    }
}
