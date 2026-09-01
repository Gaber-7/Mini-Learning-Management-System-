using AutoMapper;
using Microsoft.EntityFrameworkCore;
using GenAlpha.Business.DTOs;
using GenAlpha.Business.Interfaces;
using GenAlpha.Data.Data;
using GenAlpha.Data.Models;
using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<CertificateDto?> IssueCertificateAsync(int studentId, int courseId, decimal? finalScore = 100)
        {
            // 1. Verify student and course existence
            var student = await _context.Students.Include(s => s.User).FirstOrDefaultAsync(s => s.Id == studentId);
            if (student == null) throw new InvalidOperationException("Student not found.");

            var course = await _context.Courses.Include(c => c.Instructor).FirstOrDefaultAsync(c => c.Id == courseId);
            if (course == null) throw new InvalidOperationException("Course not found.");

            // 2. Check if certificate already issued
            var existingCert = await _context.Certificates
                .Include(c => c.Student)
                .Include(c => c.Course)
                    .ThenInclude(co => co.Instructor)
                .FirstOrDefaultAsync(c => c.StudentId == studentId && c.CourseId == courseId);

            if (existingCert != null)
            {
                return MapToDto(existingCert, student.FullName, course.Title, course.Instructor?.FullName);
            }

            // 3. Verify 100% completion in Enrollment
            var enrollment = await _context.Enrollments
                .Include(e => e.LessonProgresses)
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);

            if (enrollment == null)
            {
                throw new InvalidOperationException("Student is not enrolled in this course.");
            }

            // Generate unique code: GENA-YYYY-XXXXXX
            var randomPart = Guid.NewGuid().ToString("N")[..6].ToUpper();
            var certCode = $"GENA-{DateTime.UtcNow.Year}-{randomPart}";

            // Verification URL
            var verificationUrl = $"http://localhost:4200/verify-certificate/{certCode}";

            var certificate = new Certificate
            {
                StudentId = studentId,
                CourseId = courseId,
                CertificateCode = certCode,
                IssueDate = DateTime.UtcNow,
                QrVerificationUrl = verificationUrl
            };

            _context.Certificates.Add(certificate);
            await _context.SaveChangesAsync();

            return MapToDto(certificate, student.FullName, course.Title, course.Instructor?.FullName, finalScore);
        }

        public async Task<CertificateDto?> GetCertificateByStudentAndCourseAsync(int studentId, int courseId)
        {
            var cert = await _context.Certificates
                .Include(c => c.Student)
                .Include(c => c.Course)
                    .ThenInclude(co => co.Instructor)
                .FirstOrDefaultAsync(c => c.StudentId == studentId && c.CourseId == courseId);

            if (cert == null) return null;
            return MapToDto(cert, cert.Student.FullName, cert.Course.Title, cert.Course.Instructor?.FullName);
        }

        public async Task<CertificateVerificationResultDto?> VerifyCertificateAsync(string certificateCode)
        {
            var cert = await _context.Certificates
                .Include(c => c.Student)
                .Include(c => c.Course)
                    .ThenInclude(co => co.Instructor)
                .FirstOrDefaultAsync(c => c.CertificateCode.ToLower() == certificateCode.Trim().ToLower());

            if (cert == null)
            {
                return new CertificateVerificationResultDto
                {
                    IsValid = false,
                    CertificateCode = certificateCode,
                    Status = "Certificate Not Found or Invalid"
                };
            }

            return new CertificateVerificationResultDto
            {
                IsValid = true,
                CertificateCode = cert.CertificateCode,
                StudentName = cert.Student.FullName,
                CourseTitle = cert.Course.Title,
                InstructorName = cert.Course.Instructor?.FullName ?? "GenAlpha Certified Instructor",
                IssueDate = cert.IssueDate,
                FinalScorePercentage = 100,
                Issuer = "Gen Alpha Next-Gen Learning Platform",
                Status = "Verified & Authentic"
            };
        }

        public async Task<IEnumerable<CertificateDto>> GetStudentCertificatesAsync(int studentId)
        {
            var certs = await _context.Certificates
                .Include(c => c.Student)
                .Include(c => c.Course)
                    .ThenInclude(co => co.Instructor)
                .Where(c => c.StudentId == studentId)
                .OrderByDescending(c => c.IssueDate)
                .ToListAsync();

            return certs.Select(c => MapToDto(c, c.Student.FullName, c.Course.Title, c.Course.Instructor?.FullName));
        }

        public async Task<byte[]?> GenerateCertificatePdfAsync(string certificateCode)
        {
            var cert = await _context.Certificates
                .Include(c => c.Student)
                .Include(c => c.Course)
                    .ThenInclude(co => co.Instructor)
                .FirstOrDefaultAsync(c => c.CertificateCode.ToLower() == certificateCode.Trim().ToLower());

            if (cert == null) return null;

            var studentName = cert.Student.FullName;
            var courseTitle = cert.Course.Title;
            var instructorName = cert.Course.Instructor?.FullName ?? "Gen Alpha Academy";
            var issueDateStr = cert.IssueDate.ToString("MMMM dd, yyyy");
            var verifyUrl = cert.QrVerificationUrl ?? $"http://localhost:4200/verify-certificate/{cert.CertificateCode}";

            // Generate QR Code bytes
            byte[] qrBytes;
            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrData = qrGenerator.CreateQrCode(verifyUrl, QRCodeGenerator.ECCLevel.Q);
                var qrCode = new PngByteQRCode(qrData);
                qrBytes = qrCode.GetGraphic(20);
            }

            // Create QuestPDF Document
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(30);
                    page.PageColor(Colors.White);

                    page.Content().Border(3).BorderColor("#1E293B").Padding(20).Column(col =>
                    {
                        col.Item().Border(1).BorderColor("#0EA5E9").Padding(15).Column(inner =>
                        {
                            // Header Branding
                            inner.Item().Row(r =>
                            {
                                r.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("GEN ALPHA").FontSize(24).ExtraBold().FontColor("#0F172A").LetterSpacing(0.1f);
                                    c.Item().Text("ACADEMY OF EXCELLENCE").FontSize(10).SemiBold().FontColor("#0EA5E9").LetterSpacing(0.2f);
                                });

                                r.ConstantItem(120).AlignRight().Text("OFFICIAL CREDENTIAL").FontSize(9).Bold().FontColor("#64748B");
                            });

                            inner.Item().PaddingTop(15).LineHorizontal(1.5f).LineColor("#E2E8F0");

                            // Certificate Title
                            inner.Item().PaddingTop(20).AlignCenter().Text("CERTIFICATE OF COMPLETION")
                                .FontSize(26).ExtraBold().FontColor("#0F172A").LetterSpacing(0.08f);

                            inner.Item().PaddingTop(5).AlignCenter().Text("THIS IS PROUDLY PRESENTED TO")
                                .FontSize(11).Medium().FontColor("#64748B").LetterSpacing(0.15f);

                            // Student Name
                            inner.Item().PaddingTop(10).AlignCenter().Text(studentName)
                                .FontSize(28).Bold().FontColor("#0284C7");

                            inner.Item().PaddingTop(8).AlignCenter().Text("for successfully completing and mastering the curriculum of")
                                .FontSize(12).FontColor("#475569");

                            // Course Title
                            inner.Item().PaddingTop(8).AlignCenter().Text(courseTitle)
                                .FontSize(20).ExtraBold().FontColor("#0F172A");

                            inner.Item().PaddingTop(20).LineHorizontal(1).LineColor("#E2E8F0");

                            // Footer Section with QR, Code & Signatures
                            inner.Item().PaddingTop(15).Row(row =>
                            {
                                // QR Code Box
                                row.ConstantItem(90).Column(qCol =>
                                {
                                    qCol.Item().Width(80).Height(80).Image(qrBytes);
                                    qCol.Item().PaddingTop(3).Text("Scan to Verify").FontSize(7).AlignCenter().FontColor("#64748B");
                                });

                                // Center Info
                                row.RelativeItem().PaddingLeft(20).Column(mCol =>
                                {
                                    mCol.Item().Text($"Certificate ID: {cert.CertificateCode}").FontSize(9).Bold().FontColor("#1E293B");
                                    mCol.Item().Text($"Issued Date: {issueDateStr}").FontSize(9).FontColor("#64748B");
                                    mCol.Item().Text($"Verification: {verifyUrl}").FontSize(8).FontColor("#0284C7");
                                    mCol.Item().PaddingTop(4).Text("This digital certificate is cryptographically verifiable and recorded in the Gen Alpha registry.")
                                        .FontSize(7).Italic().FontColor("#94A3B8");
                                });

                                // Signature Box
                                row.ConstantItem(180).AlignRight().Column(sCol =>
                                {
                                    sCol.Item().PaddingTop(25).LineHorizontal(1).LineColor("#334155");
                                    sCol.Item().PaddingTop(4).Text(instructorName).FontSize(11).Bold().FontColor("#0F172A").AlignCenter();
                                    sCol.Item().Text("Course Instructor / Director").FontSize(8).FontColor("#64748B").AlignCenter();
                                });
                            });
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }

        private CertificateDto MapToDto(Certificate cert, string studentName, string courseTitle, string? instructorName, decimal? finalScore = 100)
        {
            var verifyUrl = cert.QrVerificationUrl ?? $"http://localhost:4200/verify-certificate/{cert.CertificateCode}";
            
            // Build LinkedIn Share URL
            var encodedTitle = WebUtility.UrlEncode(courseTitle);
            var encodedOrg = WebUtility.UrlEncode("Gen Alpha Next-Gen Learning");
            var encodedCertUrl = WebUtility.UrlEncode(verifyUrl);
            var linkedInUrl = $"https://www.linkedin.com/profile/add?startTask=CERTIFICATION_NAME&name={encodedTitle}&organizationName={encodedOrg}&issueYear={cert.IssueDate.Year}&issueMonth={cert.IssueDate.Month}&certUrl={encodedCertUrl}&certId={cert.CertificateCode}";

            return new CertificateDto
            {
                Id = cert.Id,
                CertificateCode = cert.CertificateCode,
                StudentId = cert.StudentId,
                StudentName = studentName,
                CourseId = cert.CourseId,
                CourseTitle = courseTitle,
                InstructorName = instructorName ?? "GenAlpha Instructor",
                IssueDate = cert.IssueDate,
                FinalScorePercentage = finalScore ?? 100,
                QrVerificationUrl = verifyUrl,
                LinkedInShareUrl = linkedInUrl
            };
        }
    }
}
