using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenAlpha.Business.DTOs
{
    public class CertificateDto
    {
        public int Id { get; set; }
        public string CertificateCode { get; set; } = string.Empty;
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public string? InstructorName { get; set; }
        public DateTime IssueDate { get; set; }
        public decimal? FinalScorePercentage { get; set; }
        public string QrVerificationUrl { get; set; } = string.Empty;
        public string LinkedInShareUrl { get; set; } = string.Empty;
    }

    public class CertificateVerificationResultDto
    {
        public bool IsValid { get; set; }
        public string CertificateCode { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public string InstructorName { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public decimal FinalScorePercentage { get; set; }
        public string Issuer { get; set; } = "Gen Alpha Next-Gen Learning Platform";
        public string Status { get; set; } = "Verified & Authentic";
    }
}
