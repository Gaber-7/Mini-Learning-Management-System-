using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GenAlpha.Data.Models
{
    public class Certificate
    {
        public int Id { get; set; }

        public int StudentId { get; set; }

        public int CourseId { get; set; }

        [Required]
        [StringLength(100)]
        public string CertificateCode { get; set; } = string.Empty;

        public DateTime IssueDate { get; set; } = DateTime.UtcNow;

        public string? QrVerificationUrl { get; set; }

        public string? PdfFilePath { get; set; }

        public virtual Student Student { get; set; } = null!;
        public virtual Course Course { get; set; } = null!;
    }
}
