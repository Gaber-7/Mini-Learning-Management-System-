using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using GenAlpha.Business.DTOs;
using GenAlpha.Business.Interfaces;
using GenAlpha.Data.Data;
using GenAlpha.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GenAlpha.Business.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IGamificationService _gamificationService;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public PaymentService(
            AppDbContext context,
            INotificationService notificationService,
            IGamificationService gamificationService,
            IConfiguration configuration,
            HttpClient? httpClient = null)
        {
            _context = context;
            _notificationService = notificationService;
            _gamificationService = gamificationService;
            _configuration = configuration;
            _httpClient = httpClient ?? new HttpClient();
        }

        public async Task<CouponResultDto> ValidateCouponAsync(string code, int courseId)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return new CouponResultDto { IsValid = false, Message = "Coupon code is required." };
            }

            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
            {
                return new CouponResultDto { IsValid = false, Message = "Course not found." };
            }

            var basePrice = course.DiscountPrice ?? course.Price;
            var coupon = await _context.Coupons
                .FirstOrDefaultAsync(c => c.Code.ToUpper() == code.Trim().ToUpper() && c.IsActive);

            if (coupon == null)
            {
                return new CouponResultDto { IsValid = false, Message = "Invalid or inactive coupon code." };
            }

            if (coupon.ExpiryDate.HasValue && coupon.ExpiryDate.Value < DateTime.UtcNow)
            {
                return new CouponResultDto { IsValid = false, Message = "This coupon has expired." };
            }

            if (coupon.CurrentUsageCount >= coupon.MaxUsageCount)
            {
                return new CouponResultDto { IsValid = false, Message = "This coupon has reached its maximum usage limit." };
            }

            if (coupon.CourseId.HasValue && coupon.CourseId.Value != courseId)
            {
                return new CouponResultDto { IsValid = false, Message = "This coupon is not valid for this specific course." };
            }

            decimal discountVal = 0;
            if (coupon.DiscountPercentage > 0)
            {
                discountVal = Math.Round(basePrice * (coupon.DiscountPercentage / 100m), 2);
            }
            else if (coupon.DiscountAmount > 0)
            {
                discountVal = Math.Min(basePrice, coupon.DiscountAmount);
            }

            var finalPrice = Math.Max(0, basePrice - discountVal);

            return new CouponResultDto
            {
                IsValid = true,
                Code = coupon.Code,
                OriginalPrice = basePrice,
                DiscountPercentage = coupon.DiscountPercentage,
                DiscountAmount = discountVal,
                FinalPrice = finalPrice,
                Message = "Coupon applied successfully!"
            };
        }

        public async Task<Coupon> CreateCouponAsync(CreateCouponDto dto)
        {
            var existing = await _context.Coupons
                .AnyAsync(c => c.Code.ToUpper() == dto.Code.Trim().ToUpper());

            if (existing)
            {
                throw new InvalidOperationException("A coupon with this code already exists.");
            }

            var coupon = new Coupon
            {
                Code = dto.Code.Trim().ToUpper(),
                DiscountPercentage = dto.DiscountPercentage,
                DiscountAmount = dto.DiscountAmount,
                ExpiryDate = dto.ExpiryDate,
                MaxUsageCount = dto.MaxUsageCount,
                CourseId = dto.CourseId,
                IsActive = true,
                CurrentUsageCount = 0
            };

            _context.Coupons.Add(coupon);
            await _context.SaveChangesAsync();

            return coupon;
        }

        public async Task<List<Coupon>> GetCouponsAsync(int? courseId = null)
        {
            var query = _context.Coupons.AsQueryable();
            if (courseId.HasValue)
            {
                query = query.Where(c => c.CourseId == null || c.CourseId == courseId.Value);
            }
            return await query.OrderByDescending(c => c.Id).ToListAsync();
        }

        public async Task<PaymentOrderResultDto> CreatePaymentOrderAsync(int studentId, CreatePaymentOrderDto dto)
        {
            var course = await _context.Courses.FindAsync(dto.CourseId);
            if (course == null) throw new InvalidOperationException("Course not found.");

            // Check if already enrolled
            var alreadyEnrolled = await _context.Enrollments
                .AnyAsync(e => e.StudentId == studentId && e.CourseId == dto.CourseId);
            if (alreadyEnrolled) throw new InvalidOperationException("You are already enrolled in this course.");

            var originalPrice = course.DiscountPrice ?? course.Price;
            decimal finalPrice = originalPrice;

            if (!string.IsNullOrWhiteSpace(dto.CouponCode))
            {
                var couponVal = await ValidateCouponAsync(dto.CouponCode, dto.CourseId);
                if (couponVal.IsValid)
                {
                    finalPrice = couponVal.FinalPrice;
                }
            }

            // Generate unique Order ID (e.g. PAYPAL-ORDER-XXXX)
            var orderId = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
            var approvalUrl = $"https://www.sandbox.paypal.com/checkoutnow?token={orderId}";

            return new PaymentOrderResultDto
            {
                OrderId = orderId,
                CourseId = course.Id,
                CourseTitle = course.Title,
                OriginalPrice = originalPrice,
                FinalAmount = finalPrice,
                Currency = "USD",
                ApprovalUrl = approvalUrl,
                Status = "Created"
            };
        }

        public async Task<PaymentResultDto> CapturePaymentAsync(int studentId, CapturePaymentDto dto)
        {
            var course = await _context.Courses
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.Id == dto.CourseId);

            if (course == null) throw new InvalidOperationException("Course not found.");

            var student = await _context.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == studentId);
            if (student == null) throw new InvalidOperationException("Student not found.");

            // Check if already enrolled
            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == dto.CourseId);

            if (enrollment != null)
            {
                return new PaymentResultDto
                {
                    Success = true,
                    CourseId = dto.CourseId,
                    Message = "Already enrolled in this course.",
                    Enrolled = true
                };
            }

            // Calculate final price
            var basePrice = course.DiscountPrice ?? course.Price;
            decimal finalAmount = basePrice;
            int? couponId = null;

            if (!string.IsNullOrWhiteSpace(dto.CouponCode))
            {
                var coupon = await _context.Coupons
                    .FirstOrDefaultAsync(c => c.Code.ToUpper() == dto.CouponCode.Trim().ToUpper() && c.IsActive);

                if (coupon != null)
                {
                    couponId = coupon.Id;
                    coupon.CurrentUsageCount++;

                    decimal discount = 0;
                    if (coupon.DiscountPercentage > 0)
                        discount = Math.Round(basePrice * (coupon.DiscountPercentage / 100m), 2);
                    else if (coupon.DiscountAmount > 0)
                        discount = Math.Min(basePrice, coupon.DiscountAmount);

                    finalAmount = Math.Max(0, basePrice - discount);
                }
            }

            var transactionId = dto.TransactionId ?? $"TXN-{Guid.NewGuid().ToString("N")[..12].ToUpper()}";

            // 1. Record CoursePayment
            var payment = new CoursePayment
            {
                StudentId = studentId,
                CourseId = dto.CourseId,
                CouponId = couponId,
                Amount = finalAmount,
                PaymentMethod = dto.PaymentMethod ?? "PayPal",
                TransactionId = transactionId,
                Status = "Completed",
                PaymentDate = DateTime.UtcNow
            };

            _context.CoursePayments.Add(payment);

            // 2. Create Enrollment
            enrollment = new Enrollment
            {
                StudentId = studentId,
                CourseId = dto.CourseId,
                EnrollmentDate = DateTime.UtcNow,
                Status = "InProgress",
                ProgressPercentage = 0
            };
            _context.Enrollments.Add(enrollment);

            // 3. Update Instructor Wallet
            if (course.InstructorId.HasValue && finalAmount > 0)
            {
                var instructorId = course.InstructorId.Value;
                var wallet = await _context.InstructorWallets.FirstOrDefaultAsync(w => w.InstructorId == instructorId);

                if (wallet == null)
                {
                    wallet = new InstructorWallet
                    {
                        InstructorId = instructorId,
                        CommissionPercentage = 70.0m,
                        TotalEarnings = 0,
                        AvailableBalance = 0,
                        WithdrawnAmount = 0
                    };
                    _context.InstructorWallets.Add(wallet);
                }

                var instructorShare = Math.Round(finalAmount * (wallet.CommissionPercentage / 100m), 2);
                wallet.TotalEarnings += instructorShare;
                wallet.AvailableBalance += instructorShare;
                wallet.LastUpdated = DateTime.UtcNow;

                // Send Real-Time SignalR Notification to Instructor
                var instructor = await _context.Instructors.FindAsync(instructorId);
                if (instructor != null)
                {
                    await _notificationService.SendNotificationAsync(new CreateNotificationDto
                    {
                        UserId = instructorId,
                        Title = "طالب جديد اشترى كورس لك! 💰",
                        Message = $"قام الطالب {student.FullName} بالاشتراك في كورس ({course.Title}). أرباحك: ${instructorShare:F2}.",
                        NotificationType = "PaymentSuccess",
                        ActionUrl = $"/instructor/courses"
                    });
                }
            }

            await _context.SaveChangesAsync();

            // 4. Send Confirmation Notification to Student
            await _notificationService.SendNotificationAsync(new CreateNotificationDto
            {
                UserId = studentId,
                Title = "تم تفعيل اشتراكك بنجاح! 🎓",
                Message = $"تهانينا! لقد تم تسجيلك بنجاح في كورس ({course.Title}). ابدأ التعلم الآن!",
                NotificationType = "General",
                ActionUrl = $"/student/courses/{course.Id}"
            });

            // 5. Award initial XP for starting course
            await _gamificationService.AwardXPAsync(studentId, 25, $"Enrolled in {course.Title}");

            return new PaymentResultDto
            {
                Success = true,
                PaymentId = payment.Id,
                CourseId = course.Id,
                TransactionId = transactionId,
                AmountPaid = finalAmount,
                Message = "Payment captured and enrollment completed successfully.",
                Enrolled = true
            };
        }

        public async Task<InstructorWalletDto> GetInstructorWalletAsync(int instructorId)
        {
            var instructor = await _context.Instructors.FindAsync(instructorId);
            if (instructor == null) throw new InvalidOperationException("Instructor not found.");

            var wallet = await _context.InstructorWallets.FirstOrDefaultAsync(w => w.InstructorId == instructorId);
            if (wallet == null)
            {
                wallet = new InstructorWallet
                {
                    InstructorId = instructorId,
                    CommissionPercentage = 70.0m,
                    TotalEarnings = 0,
                    AvailableBalance = 0,
                    WithdrawnAmount = 0,
                    LastUpdated = DateTime.UtcNow
                };
                _context.InstructorWallets.Add(wallet);
                await _context.SaveChangesAsync();
            }

            // Get recent payments for instructor's courses
            var instructorCourseIds = await _context.Courses
                .Where(c => c.InstructorId == instructorId)
                .Select(c => c.Id)
                .ToListAsync();

            var recentPayments = await _context.CoursePayments
                .Include(p => p.Course)
                .Include(p => p.Student)
                .Where(p => instructorCourseIds.Contains(p.CourseId) && p.Status == "Completed")
                .OrderByDescending(p => p.PaymentDate)
                .Take(20)
                .Select(p => new CoursePaymentSummaryDto
                {
                    PaymentId = p.Id,
                    CourseTitle = p.Course.Title,
                    StudentName = p.Student.FullName,
                    Amount = p.Amount,
                    InstructorShare = Math.Round(p.Amount * (wallet.CommissionPercentage / 100m), 2),
                    PaymentDate = p.PaymentDate,
                    Status = p.Status
                })
                .ToListAsync();

            return new InstructorWalletDto
            {
                InstructorId = instructor.Id,
                InstructorName = instructor.FullName,
                TotalEarnings = wallet.TotalEarnings,
                AvailableBalance = wallet.AvailableBalance,
                WithdrawnAmount = wallet.WithdrawnAmount,
                CommissionPercentage = wallet.CommissionPercentage,
                LastUpdated = wallet.LastUpdated,
                RecentPayments = recentPayments
            };
        }

        public async Task<bool> RequestPayoutAsync(int instructorId, PayoutRequestDto dto)
        {
            var wallet = await _context.InstructorWallets.FirstOrDefaultAsync(w => w.InstructorId == instructorId);
            if (wallet == null) throw new InvalidOperationException("Wallet not found.");

            if (dto.Amount <= 0 || dto.Amount > wallet.AvailableBalance)
            {
                throw new InvalidOperationException("Invalid payout amount or insufficient available balance.");
            }

            wallet.AvailableBalance -= dto.Amount;
            wallet.WithdrawnAmount += dto.Amount;
            wallet.LastUpdated = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Send notification
            await _notificationService.SendNotificationAsync(new CreateNotificationDto
            {
                UserId = instructorId,
                Title = "تم استلام طلب سحب الأرباح 💸",
                Message = $"تم معالجة طلب سحب مبلغ ${dto.Amount:F2} إلى حساب PayPal ({dto.PayoutAccount}).",
                NotificationType = "PaymentSuccess",
                ActionUrl = "/instructor/wallet"
            });

            return true;
        }
    }
}
