import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CertificateDto, CertificateVerificationResultDto } from '../../Models/GenAlpha';
import { AuthService } from './auth-service';

@Injectable({
  providedIn: 'root'
})
export class CertificateService {
  private apiUrl = 'https://localhost:7070/api/Certificates';

  constructor(private http: HttpClient, private authService: AuthService) {}

  private getAuthHeaders(): HttpHeaders {
    const token = this.authService.getToken();
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
  }

  issueCertificate(courseId: number, studentId?: number, finalScore: number = 100): Observable<CertificateDto> {
    const body = { courseId, studentId: studentId || 0, finalScore };
    return this.http.post<CertificateDto>(`${this.apiUrl}/issue`, body, { headers: this.getAuthHeaders() });
  }

  verifyCertificate(code: string): Observable<CertificateVerificationResultDto> {
    return this.http.get<CertificateVerificationResultDto>(`${this.apiUrl}/verify/${code}`);
  }

  getStudentCertificates(studentId: number): Observable<CertificateDto[]> {
    return this.http.get<CertificateDto[]>(`${this.apiUrl}/student/${studentId}`, { headers: this.getAuthHeaders() });
  }

  getCertificateForCourse(courseId: number): Observable<CertificateDto> {
    return this.http.get<CertificateDto>(`${this.apiUrl}/course/${courseId}`, { headers: this.getAuthHeaders() });
  }

  downloadCertificatePdf(code: string): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/download/${code}`, { responseType: 'blob' });
  }
}
