using Microsoft.EntityFrameworkCore;
using GenAlpha.Business.DTOs;
using GenAlpha.Business.Interfaces;
using GenAlpha.Data.Data;
using GenAlpha.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenAlpha.Business.Services
{
    public class GamificationService : IGamificationService
    {
        private readonly AppDbContext _context;

        public GamificationService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<StudentGamificationDto> GetStudentProfileAsync(int studentId)
        {
            await EnsureBadgesSeededAsync();

            var student = await _context.Students.FindAsync(studentId);
            if (student == null) throw new InvalidOperationException("Student not found.");

            var gamification = await _context.StudentGamifications
                .FirstOrDefaultAsync(g => g.StudentId == studentId);

            if (gamification == null)
            {
                gamification = new StudentGamification
                {
                    StudentId = studentId,
                    TotalXP = 50, // Welcome XP bonus
                    CurrentStreakDays = 1,
                    LongestStreakDays = 1,
                    LastActiveDate = DateTime.UtcNow
                };
                _context.StudentGamifications.Add(gamification);
                await _context.SaveChangesAsync();
            }

            var earnedBadges = await _context.StudentBadges
                .Include(sb => sb.Badge)
                .Where(sb => sb.StudentId == studentId)
                .OrderByDescending(sb => sb.EarnedDate)
                .Select(sb => new BadgeDto
                {
                    Id = sb.Badge.Id,
                    Code = sb.Badge.Code,
                    Title = sb.Badge.Title,
                    Description = sb.Badge.Description,
                    IconUrl = sb.Badge.IconUrl,
                    XPReward = sb.Badge.XPReward,
                    IsEarned = true,
                    EarnedDate = sb.EarnedDate
                })
                .ToListAsync();

            return BuildProfileDto(student.FullName, gamification, earnedBadges);
        }

        public async Task<StudentGamificationDto> AwardXPAsync(int studentId, int amount, string reason)
        {
            var gamification = await _context.StudentGamifications
                .FirstOrDefaultAsync(g => g.StudentId == studentId);

            if (gamification == null)
            {
                var profile = await GetStudentProfileAsync(studentId);
                gamification = await _context.StudentGamifications.FirstAsync(g => g.StudentId == studentId);
            }

            gamification.TotalXP += amount;
            await UpdateStreakInternalAsync(gamification);
            await _context.SaveChangesAsync();

            // Check milestone badges
            await CheckAndAwardMilestoneBadgesAsync(studentId);

            return await GetStudentProfileAsync(studentId);
        }

        public async Task<StudentGamificationDto> UpdateStreakAsync(int studentId)
        {
            var gamification = await _context.StudentGamifications
                .FirstOrDefaultAsync(g => g.StudentId == studentId);

            if (gamification == null)
            {
                return await GetStudentProfileAsync(studentId);
            }

            await UpdateStreakInternalAsync(gamification);
            await _context.SaveChangesAsync();

            return await GetStudentProfileAsync(studentId);
        }

        public async Task<List<BadgeDto>> GetAllBadgesWithStudentStatusAsync(int studentId)
        {
            await EnsureBadgesSeededAsync();

            var allBadges = await _context.Badges.ToListAsync();
            var earnedBadges = await _context.StudentBadges
                .Where(sb => sb.StudentId == studentId)
                .ToDictionaryAsync(sb => sb.BadgeId, sb => sb.EarnedDate);

            return allBadges.Select(b => new BadgeDto
            {
                Id = b.Id,
                Code = b.Code,
                Title = b.Title,
                Description = b.Description,
                IconUrl = b.IconUrl,
                XPReward = b.XPReward,
                IsEarned = earnedBadges.ContainsKey(b.Id),
                EarnedDate = earnedBadges.TryGetValue(b.Id, out var date) ? date : null
            }).ToList();
        }

        public async Task<List<LeaderboardItemDto>> GetLeaderboardAsync(int topCount = 10)
        {
            await EnsureBadgesSeededAsync();

            var studentsWithXP = await _context.StudentGamifications
                .Include(g => g.Student)
                .OrderByDescending(g => g.TotalXP)
                .Take(topCount)
                .ToListAsync();

            var studentIds = studentsWithXP.Select(g => g.StudentId).ToList();
            var badgesCountMap = await _context.StudentBadges
                .Where(sb => studentIds.Contains(sb.StudentId))
                .GroupBy(sb => sb.StudentId)
                .Select(g => new { StudentId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.StudentId, x => x.Count);

            var leaderboard = new List<LeaderboardItemDto>();
            int rank = 1;

            foreach (var item in studentsWithXP)
            {
                leaderboard.Add(new LeaderboardItemDto
                {
                    Rank = rank++,
                    StudentId = item.StudentId,
                    StudentName = item.Student.FullName,
                    TotalXP = item.TotalXP,
                    Level = CalculateLevel(item.TotalXP),
                    StreakDays = item.CurrentStreakDays,
                    BadgesCount = badgesCountMap.TryGetValue(item.StudentId, out var count) ? count : 0
                });
            }

            return leaderboard;
        }

        public async Task<List<BadgeDto>> CheckAndAwardMilestoneBadgesAsync(int studentId)
        {
            await EnsureBadgesSeededAsync();

            var newlyEarned = new List<BadgeDto>();
            var gamification = await _context.StudentGamifications.FirstOrDefaultAsync(g => g.StudentId == studentId);
            if (gamification == null) return newlyEarned;

            var existingBadgeIds = await _context.StudentBadges
                .Where(sb => sb.StudentId == studentId)
                .Select(sb => sb.Badge.Code)
                .ToListAsync();

            var allBadges = await _context.Badges.ToDictionaryAsync(b => b.Code, b => b);

            // 1. Check FIRST_LESSON
            if (!existingBadgeIds.Contains("FIRST_LESSON"))
            {
                var hasCompletedLesson = await _context.LessonProgresses
                    .AnyAsync(lp => lp.Enrollment.StudentId == studentId && lp.IsCompleted);
                if (hasCompletedLesson && allBadges.TryGetValue("FIRST_LESSON", out var badge))
                {
                    await AwardBadgeAsync(studentId, badge);
                    newlyEarned.Add(MapBadge(badge, true, DateTime.UtcNow));
                }
            }

            // 2. Check QUIZ_MASTER
            if (!existingBadgeIds.Contains("QUIZ_MASTER"))
            {
                var perfectScoreQuizzes = await _context.QuizAttempts
                    .CountAsync(qa => qa.StudentId == studentId && qa.Score >= 90);
                if (perfectScoreQuizzes >= 1 && allBadges.TryGetValue("QUIZ_MASTER", out var badge))
                {
                    await AwardBadgeAsync(studentId, badge);
                    newlyEarned.Add(MapBadge(badge, true, DateTime.UtcNow));
                }
            }

            // 3. Check STREAK_7
            if (!existingBadgeIds.Contains("STREAK_7") && gamification.CurrentStreakDays >= 7)
            {
                if (allBadges.TryGetValue("STREAK_7", out var badge))
                {
                    await AwardBadgeAsync(studentId, badge);
                    newlyEarned.Add(MapBadge(badge, true, DateTime.UtcNow));
                }
            }

            // 4. Check COURSE_GRADUATE
            if (!existingBadgeIds.Contains("COURSE_GRADUATE"))
            {
                var completedCourses = await _context.Enrollments
                    .CountAsync(e => e.StudentId == studentId && e.Status == "Completed");
                if (completedCourses >= 1 && allBadges.TryGetValue("COURSE_GRADUATE", out var badge))
                {
                    await AwardBadgeAsync(studentId, badge);
                    newlyEarned.Add(MapBadge(badge, true, DateTime.UtcNow));
                }
            }

            // 5. Check XP_PRO_500
            if (!existingBadgeIds.Contains("XP_PRO_500") && gamification.TotalXP >= 500)
            {
                if (allBadges.TryGetValue("XP_PRO_500", out var badge))
                {
                    await AwardBadgeAsync(studentId, badge);
                    newlyEarned.Add(MapBadge(badge, true, DateTime.UtcNow));
                }
            }

            return newlyEarned;
        }

        private async Task AwardBadgeAsync(int studentId, Badge badge)
        {
            _context.StudentBadges.Add(new StudentBadge
            {
                StudentId = studentId,
                BadgeId = badge.Id,
                EarnedDate = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        private async Task UpdateStreakInternalAsync(StudentGamification g)
        {
            var now = DateTime.UtcNow;
            var lastActive = g.LastActiveDate.Date;
            var today = now.Date;

            if (today > lastActive)
            {
                if (today == lastActive.AddDays(1))
                {
                    g.CurrentStreakDays++;
                    if (g.CurrentStreakDays > g.LongestStreakDays)
                    {
                        g.LongestStreakDays = g.CurrentStreakDays;
                    }
                }
                else if (today > lastActive.AddDays(1))
                {
                    g.CurrentStreakDays = 1;
                }
                g.LastActiveDate = now;
            }
        }

        private int CalculateLevel(int totalXP)
        {
            if (totalXP < 150) return 1;
            if (totalXP < 400) return 2;
            if (totalXP < 800) return 3;
            if (totalXP < 1500) return 4;
            return 5;
        }

        private string GetLevelTitle(int level)
        {
            return level switch
            {
                1 => "مبتدئ طموح (Rookie Learner)",
                2 => "متعلم نشط (Active Scholar)",
                3 => "مطور واعد (Rising Dev)",
                4 => "محترف أكاديمي (Pro Coder)",
                _ => "أسطورة المنصة (GenAlpha Legend)"
            };
        }

        private StudentGamificationDto BuildProfileDto(string studentName, StudentGamification g, List<BadgeDto> badges)
        {
            var level = CalculateLevel(g.TotalXP);
            var (minXp, nextLevelXp) = level switch
            {
                1 => (0, 150),
                2 => (150, 400),
                3 => (400, 800),
                4 => (800, 1500),
                _ => (1500, 3000)
            };

            var currentLevelProgress = Math.Max(0, g.TotalXP - minXp);
            var span = nextLevelXp - minXp;
            var percentage = Math.Min(100, Math.Round(((decimal)currentLevelProgress / span) * 100, 1));

            return new StudentGamificationDto
            {
                StudentId = g.StudentId,
                StudentName = studentName,
                TotalXP = g.TotalXP,
                Level = level,
                LevelTitle = GetLevelTitle(level),
                CurrentStreakDays = g.CurrentStreakDays,
                LongestStreakDays = g.LongestStreakDays,
                LastActiveDate = g.LastActiveDate,
                NextLevelXP = nextLevelXp,
                CurrentLevelProgressXP = currentLevelProgress,
                ProgressToNextLevelPercentage = percentage,
                EarnedBadges = badges
            };
        }

        private BadgeDto MapBadge(Badge b, bool isEarned, DateTime? earnedDate)
        {
            return new BadgeDto
            {
                Id = b.Id,
                Code = b.Code,
                Title = b.Title,
                Description = b.Description,
                IconUrl = b.IconUrl,
                XPReward = b.XPReward,
                IsEarned = isEarned,
                EarnedDate = earnedDate
            };
        }

        private async Task EnsureBadgesSeededAsync()
        {
            if (await _context.Badges.AnyAsync()) return;

            var defaultBadges = new List<Badge>
            {
                new Badge { Code = "FIRST_LESSON", Title = "الخطوة الأولى 🚀", Description = "أكملت أول درس بنجاح على المنصة.", IconUrl = "bi-rocket-takeoff-fill", XPReward = 50 },
                new Badge { Code = "QUIZ_MASTER", Title = "عبقري الاختبارات 🧠", Description = "حصلت على نسبة 90% أو أعلى في اختبار.", IconUrl = "bi-patch-check-fill", XPReward = 100 },
                new Badge { Code = "STREAK_7", Title = "شعلة الالتزام 🔥", Description = "حافظت على سلسلة تعلم متواصلة لمدة 7 أيام.", IconUrl = "bi-fire", XPReward = 150 },
                new Badge { Code = "COURSE_GRADUATE", Title = "خريج الدورة 🎓", Description = "أتممت دراسة كورس كامل بنسبة 100%.", IconUrl = "bi-mortarboard-fill", XPReward = 250 },
                new Badge { Code = "XP_PRO_500", Title = "نخبة المتعلمين 💎", Description = "جمعت أكثر من 500 نقطة خبرة (XP).", IconUrl = "bi-gem", XPReward = 200 }
            };

            _context.Badges.AddRange(defaultBadges);
            await _context.SaveChangesAsync();
        }
    }
}
