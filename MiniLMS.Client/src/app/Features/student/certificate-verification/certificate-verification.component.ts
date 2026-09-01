import { Component, OnInit, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CertificateService } from '../../../Core/Services/certificate.service';
import { CertificateVerificationResultDto } from '../../../Models/GenAlpha';

@Component({
  selector: 'app-certificate-verification',
  standalone: true,
  imports: [CommonModule, RouterLink, DatePipe],
  templateUrl: './certificate-verification.component.html',
  styleUrl: './certificate-verification.component.css'
})
export class CertificateVerificationComponent implements OnInit {
  certificateCode = '';
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);
  readonly result = signal<CertificateVerificationResultDto | null>(null);
  readonly downloading = signal(false);

  constructor(
    private route: ActivatedRoute,
    private certificateService: CertificateService
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.certificateCode = params['code'] || '';
      if (this.certificateCode) {
        this.verify();
      } else {
        this.loading.set(false);
        this.error.set('كود الشهادة غير موجود في الرابط.');
      }
    });
  }

  verify(): void {
    this.loading.set(true);
    this.error.set(null);

    this.certificateService.verifyCertificate(this.certificateCode).subscribe({
      next: (res) => {
        this.result.set(res);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set('لم يتم العثور على شهادة صالحة بهذا الكود أو أن الشهادة غير معتمدة.');
      }
    });
  }

  downloadPdf(): void {
    if (!this.certificateCode) return;
    this.downloading.set(true);

    this.certificateService.downloadCertificatePdf(this.certificateCode).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `GenAlpha_Certificate_${this.certificateCode}.pdf`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
        this.downloading.set(false);
      },
      error: () => {
        this.downloading.set(false);
        alert('حدث خطأ أثناء تحميل ملف الـ PDF.');
      }
    });
  }

  getLinkedInShareUrl(): string {
    const r = this.result();
    if (!r) return '#';
    const certUrl = window.location.href;
    const certDate = new Date(r.issueDate);
    const year = certDate.getFullYear();
    const month = certDate.getMonth() + 1;
    const name = encodeURIComponent(r.courseTitle);
    const org = encodeURIComponent('Gen Alpha Platform');
    const encodedUrl = encodeURIComponent(certUrl);
    const id = encodeURIComponent(r.certificateCode);

    return `https://www.linkedin.com/profile/add?startTask=CERTIFICATION_NAME&name=${name}&organizationName=${org}&issueYear=${year}&issueMonth=${month}&certUrl=${encodedUrl}&certId=${id}`;
  }
}
