using GenAlpha.Business.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GenAlpha.Business.Interfaces
{
    public interface IGamificationService
    {
        Task<StudentGamificationDto> GetStudentProfileAsync(int studentId);
        Task<StudentGamificationDto> AwardXPAsync(int studentId, int amount, string reason);
        Task<StudentGamificationDto> UpdateStreakAsync(int studentId);
        Task<List<BadgeDto>> GetAllBadgesWithStudentStatusAsync(int studentId);
        Task<List<LeaderboardItemDto>> GetLeaderboardAsync(int topCount = 10);
        Task<List<BadgeDto>> CheckAndAwardMilestoneBadgesAsync(int studentId);
    }
}
