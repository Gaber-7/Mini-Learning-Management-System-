import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  CapturePaymentDto,
  CouponResultDto,
  CreatePaymentOrderDto,
  InstructorWalletDto,
  PaymentOrderResultDto,
  PaymentResultDto
} from '../../Models/GenAlpha';
import { AuthService } from './auth-service';

@Injectable({
  providedIn: 'root'
})
export class PaymentService {
  private paymentApiUrl = 'https://localhost:7070/api/Payments';
  private couponApiUrl = 'https://localhost:7070/api/Coupons';

  constructor(private http: HttpClient, private authService: AuthService) {}

  private getAuthHeaders(): HttpHeaders {
    const token = this.authService.getToken();
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
  }

  validateCoupon(code: string, courseId: number): Observable<CouponResultDto> {
    return this.http.post<CouponResultDto>(`${this.couponApiUrl}/validate`, { code, courseId });
  }

  createPaymentOrder(dto: CreatePaymentOrderDto): Observable<PaymentOrderResultDto> {
    return this.http.post<PaymentOrderResultDto>(`${this.paymentApiUrl}/create-order`, dto, { headers: this.getAuthHeaders() });
  }

  capturePayment(dto: CapturePaymentDto): Observable<PaymentResultDto> {
    return this.http.post<PaymentResultDto>(`${this.paymentApiUrl}/capture`, dto, { headers: this.getAuthHeaders() });
  }

  getInstructorWallet(): Observable<InstructorWalletDto> {
    return this.http.get<InstructorWalletDto>(`${this.paymentApiUrl}/instructor/wallet`, { headers: this.getAuthHeaders() });
  }

  requestPayout(amount: number, payoutAccount: string, note?: string): Observable<any> {
    const body = { amount, payoutAccount, note };
    return this.http.post(`${this.paymentApiUrl}/instructor/payout`, body, { headers: this.getAuthHeaders() });
  }
}
