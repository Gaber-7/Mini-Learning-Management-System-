using System;
using System.Collections.Generic;

namespace GenAlpha.Business.DTOs
{
    public class StudentGamificationDto
    {
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int TotalXP { get; set; }
        public int Level { get; set; }
        public string LevelTitle { get; set; } = "Rookie Learner";
        public int CurrentStreakDays { get; set; }
        public int LongestStreakDays { get; set; }
        public DateTime LastActiveDate { get; set; }
        public int NextLevelXP { get; set; }
        public int CurrentLevelProgressXP { get; set; }
        public decimal ProgressToNextLevelPercentage { get; set; }
        public List<BadgeDto> EarnedBadges { get; set; } = new List<BadgeDto>();
    }

    public class BadgeDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconUrl { get; set; } = string.Empty;
        public int XPReward { get; set; }
        public bool IsEarned { get; set; }
        public DateTime? EarnedDate { get; set; }
    }

    public class LeaderboardItemDto
    {
        public int Rank { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public int TotalXP { get; set; }
        public int Level { get; set; }
        public int StreakDays { get; set; }
        public int BadgesCount { get; set; }
    }

    public class AddXpRequestDto
    {
        public int StudentId { get; set; }
        public int Amount { get; set; }
        public string Reason { get; set; } = "Lesson Completed";
    }
}
