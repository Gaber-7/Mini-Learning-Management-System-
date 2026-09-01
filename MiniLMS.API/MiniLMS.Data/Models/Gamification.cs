using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenAlpha.Data.Models
{
    public class StudentGamification
    {
        [Key]
        [ForeignKey("Student")]
        public int StudentId { get; set; }

        public int TotalXP { get; set; } = 0;

        public int CurrentStreakDays { get; set; } = 1;

        public int LongestStreakDays { get; set; } = 1;

        public DateTime LastActiveDate { get; set; } = DateTime.UtcNow;

        public virtual Student Student { get; set; } = null!;
    }

    public class Badge
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty; // e.g. "FIRST_LESSON", "STREAK_7", "QUIZ_MASTER"

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string IconUrl { get; set; } = string.Empty;

        public int XPReward { get; set; } = 50;

        public virtual ICollection<StudentBadge> StudentBadges { get; set; } = new List<StudentBadge>();
    }

    public class StudentBadge
    {
        public int Id { get; set; }

        public int StudentId { get; set; }

        public int BadgeId { get; set; }

        public DateTime EarnedDate { get; set; } = DateTime.UtcNow;

        public virtual Student Student { get; set; } = null!;
        public virtual Badge Badge { get; set; } = null!;
    }
}
