using GenAlpha.Business.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenAlpha.Business.Interfaces
{
    public interface ICertificateService
    {
        Task<CertificateDto?> IssueCertificateAsync(int studentId, int courseId, decimal? finalScore = 100);
        Task<CertificateDto?> GetCertificateByStudentAndCourseAsync(int studentId, int courseId);
        Task<CertificateVerificationResultDto?> VerifyCertificateAsync(string certificateCode);
        Task<IEnumerable<CertificateDto>> GetStudentCertificatesAsync(int studentId);
        Task<byte[]?> GenerateCertificatePdfAsync(string certificateCode);
    }
}
