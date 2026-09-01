// File: CertificateService.cs
using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MiniLMS.Business.DTOs;
using MiniLMS.Data.Data;
using MiniLMS.Data.Models;
using MiniLMS.Business.Helpers;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GenAlpha.Business.Services
{
    public class CertificateService : ICertificateService
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public CertificateService(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // Placeholder: in real code verify 100% completion of all lessons/quizzes
        private async Task<bool> HasStudentCompletedCourseAsync(int studentId, int courseId)
        {
            // TODO: replace with actual progress check
            return await Task.FromResult(true);
        }

        private string GenerateCertificateCode()
        {
            return $"GENALPHA-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0,4).ToUpper()}";
        }

        private byte[] GenerateQrCode(string url)
        {
            using var generator = new QRCodeGenerator();
            var data = generator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
            var qr = new PngByteQRCode(data);
            return qr.GetGraphic(20);
        }

        private byte[] GeneratePdf(Certificate cert, byte[] qrImageBytes)
        {
            var student = _context.Students.Find(cert.StudentId);
            var course = _context.Courses.Find(cert.CourseId);
            var qrBase64 = Convert.ToBase64String(qrImageBytes);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(50);
                    page.Content().Column(col =>
                    {
                        col.Item().Text("Certificate of Completion")
                            .FontSize(36).Bold().AlignCenter();
                        col.Spacing(20);
                        col.Item().Text($"This certifies that {student?.FullName ?? "[Student]"}")
                            .FontSize(20).AlignCenter();
                        col.Item().Text("has successfully completed the course")
                            .FontSize(20).AlignCenter();
                        col.Item().Text($"{course?.Title ?? "[Course]"}")
                            .FontSize(24).Bold().AlignCenter();
                        col.Spacing(20);
                        col.Item().Text($"Issued on: {cert.IssueDate:yyyy-MM-dd}")
                            .FontSize(14).AlignRight();
                        col.Item().Text($"Certificate Code: {cert.CertificateCode}")
                            .FontSize(14).AlignRight();
                        col.Spacing(30);
                        col.Item().AlignCenter().Image(qrBase64, ImageScaling.FitArea);
                    });
                });
            });

            return document.GeneratePdf();
        }

        public async Task<CertificateDto> IssueCertificateAsync(int studentId, int courseId)
        {
            if (!await HasStudentCompletedCourseAsync(studentId, courseId))
                throw new InvalidOperationException("Student has not completed the course.");

            var cert = new Certificate
            {
                StudentId = studentId,
                CourseId = courseId,
                IssueDate = DateTime.UtcNow,
                CertificateCode = GenerateCertificateCode()
            };

            var verificationUrl = $"https://genalpha.com/api/certificates/verify/{cert.CertificateCode}";
            var qrBytes = GenerateQrCode(verificationUrl);
            cert.QrVerificationUrl = verificationUrl;

            _context.Certificates.Add(cert);
            await _context.SaveChangesAsync();

            var pdfBytes = GeneratePdf(cert, qrBytes);
            var dto = _mapper.Map<CertificateDto>(cert);
            dto.QrImageBase64 = Convert.ToBase64String(qrBytes);
            dto.PdfBase64 = Convert.ToBase64String(pdfBytes);
            dto.LinkedInShareUrl = $"https://www.linkedin.com/profile/add?startTask=CERTIFICATE&code={cert.CertificateCode}";
            return dto;
        }

        public async Task<CertificateVerificationResultDto> VerifyCertificateAsync(string code)
        {
            var cert = await _context.Certificates
                .Include(c => c.Student)
                .Include(c => c.Course)
                .FirstOrDefaultAsync(c => c.CertificateCode == code);
            if (cert == null) return null;

            return new CertificateVerificationResultDto
            {
                CertificateCode = cert.CertificateCode,
                StudentName = cert.Student.FullName,
                CourseTitle = cert.Course.Title,
                IssueDate = cert.IssueDate,
                IsValid = true,
                VerificationUrl = cert.QrVerificationUrl
            };
        }

        public async Task<CertificateDto[]> GetCertificatesByStudentAsync(int studentId)
        {
            var certs = await _context.Certificates
                .Where(c => c.StudentId == studentId)
                .ToListAsync();
            return _mapper.Map<CertificateDto[]>(certs);
        }
    }
}
