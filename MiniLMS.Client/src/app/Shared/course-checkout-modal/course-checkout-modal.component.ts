import { Component, EventEmitter, Input, Output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { PaymentService } from '../../Core/Services/payment.service';
import { Course } from '../../Models/Course';
import { CouponResultDto, PaymentOrderResultDto, PaymentResultDto } from '../../Models/GenAlpha';

@Component({
  selector: 'app-course-checkout-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './course-checkout-modal.component.html',
  styleUrl: './course-checkout-modal.component.css'
})
export class CourseCheckoutModalComponent {
  @Input() course: any | null = null;
  @Output() close = new EventEmitter<void>();
  @Output() enrolled = new EventEmitter<PaymentResultDto>();

  couponCodeInput = '';
  readonly couponResult = signal<CouponResultDto | null>(null);
  readonly validatingCoupon = signal(false);
  readonly processingPayment = signal(false);
  readonly paymentSuccess = signal(false);
  readonly errorMessage = signal<string | null>(null);

  constructor(private paymentService: PaymentService) {}

  get originalPrice(): number {
    return this.course?.discountPrice ?? this.course?.price ?? 0;
  }

  get finalPrice(): number {
    const res = this.couponResult();
    if (res && res.isValid) {
      return res.finalPrice;
    }
    return this.originalPrice;
  }

  applyCoupon(): void {
    if (!this.couponCodeInput.trim() || !this.course) return;

    this.validatingCoupon.set(true);
    this.errorMessage.set(null);

    this.paymentService.validateCoupon(this.couponCodeInput.trim(), this.course.id).subscribe({
      next: (res: CouponResultDto) => {
        this.couponResult.set(res);
        this.validatingCoupon.set(false);
        if (!res.isValid) {
          this.errorMessage.set(res.message || 'الكوبون غير صالح');
        }
      },
      error: () => {
        this.validatingCoupon.set(false);
        this.errorMessage.set('فشل التحقق من الكوبون.');
      }
    });
  }

  payWithPayPal(): void {
    if (!this.course) return;

    this.processingPayment.set(true);
    this.errorMessage.set(null);

    const couponRes = this.couponResult();

    // 1. Create order
    this.paymentService.createPaymentOrder({
      courseId: this.course.id,
      couponCode: couponRes?.isValid ? couponRes.code : undefined,
      paymentMethod: 'PayPal'
    }).subscribe({
      next: (order: PaymentOrderResultDto) => {
        // 2. Capture payment
        this.paymentService.capturePayment({
          orderId: order.orderId,
          courseId: this.course.id,
          couponCode: couponRes?.isValid ? couponRes.code : undefined,
          paymentMethod: 'PayPal'
        }).subscribe({
          next: (captureRes: PaymentResultDto) => {
            this.processingPayment.set(false);
            this.paymentSuccess.set(true);
            this.enrolled.emit(captureRes);
            setTimeout(() => {
              this.close.emit();
            }, 2500);
          },
          error: (err: any) => {
            this.processingPayment.set(false);
            this.errorMessage.set('فشلت معالجة الدفع: ' + (err.error?.message || err.message));
          }
        });
      },
      error: (err: any) => {
        this.processingPayment.set(false);
        this.errorMessage.set('تعذر إنشاء طلب الدفع: ' + (err.error?.message || err.message));
      }
    });
  }

  closeModal(): void {
    this.close.emit();
  }
}
