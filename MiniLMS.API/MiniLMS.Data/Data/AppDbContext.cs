using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using GenAlpha.Data.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace GenAlpha.Data.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<LessonProgress> LessonProgresses { get; set; }

        // Quizzes
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<QuizQuestion> QuizQuestions { get; set; }
        public DbSet<QuizOption> QuizOptions { get; set; }
        public DbSet<QuizAttempt> QuizAttempts { get; set; }
        public DbSet<QuizAnswer> QuizAnswers { get; set; }

        // Assignments
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<AssignmentSubmission> AssignmentSubmissions { get; set; }

        // Q&A / Discussions
        public DbSet<LessonQuestion> LessonQuestions { get; set; }
        public DbSet<LessonReply> LessonReplies { get; set; }

        // Reviews
        public DbSet<CourseReview> CourseReviews { get; set; }

        // Gen Alpha: Certificates, Gamification, Notifications, Monetization
        public DbSet<Certificate> Certificates { get; set; }
        public DbSet<StudentGamification> StudentGamifications { get; set; }
        public DbSet<Badge> Badges { get; set; }
        public DbSet<StudentBadge> StudentBadges { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<CoursePayment> CoursePayments { get; set; }
        public DbSet<InstructorWallet> InstructorWallets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Student User (1-to-1)
            modelBuilder.Entity<Student>()
                .HasOne(s => s.User)
                .WithOne(u => u.Student)
                .HasForeignKey<Student>(s => s.Id)
                .OnDelete(DeleteBehavior.Cascade);

            // Instructor User (1-to-1)
            modelBuilder.Entity<Instructor>()
                .HasOne(i => i.User)
                .WithOne(u => u.Instructor)
                .HasForeignKey<Instructor>(i => i.Id)
                .OnDelete(DeleteBehavior.Cascade);

            // Course - Instructor (Many-to-1 optional)
            modelBuilder.Entity<Course>()
                .HasOne(c => c.Instructor)
                .WithMany(i => i.Courses)
                .HasForeignKey(c => c.InstructorId)
                .OnDelete(DeleteBehavior.SetNull);

            // Course Sections (Cascade)
            modelBuilder.Entity<Course>()
                .HasMany(c => c.Sections)
                .WithOne(s => s.Course)
                .HasForeignKey(s => s.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Section Lessons (Cascade)
            modelBuilder.Entity<Section>()
                .HasMany(s => s.Lessons)
                .WithOne(l => l.Section)
                .HasForeignKey(l => l.SectionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Course Direct Lessons
            modelBuilder.Entity<Course>()
                .HasMany(c => c.Lessons)
                .WithOne(l => l.Course)
                .HasForeignKey(l => l.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Enrollments
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany()
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Lesson Progresses
            modelBuilder.Entity<LessonProgress>()
                .HasOne(lp => lp.Enrollment)
                .WithMany(e => e.LessonProgresses)
                .HasForeignKey(lp => lp.EnrollmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LessonProgress>()
                .HasOne(lp => lp.Lesson)
                .WithMany(l => l.LessonProgresses)
                .HasForeignKey(lp => lp.LessonId)
                .OnDelete(DeleteBehavior.Restrict);


            // Quizzes Relationships
            modelBuilder.Entity<Course>()
                .HasMany(c => c.Quizzes)
                .WithOne(q => q.Course)
                .HasForeignKey(q => q.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Section>()
                .HasMany(s => s.Quizzes)
                .WithOne(q => q.Section)
                .HasForeignKey(q => q.SectionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Quiz>()
                .HasMany(q => q.Questions)
                .WithOne(qq => qq.Quiz)
                .HasForeignKey(qq => qq.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuizQuestion>()
                .HasMany(qq => qq.Options)
                .WithOne(qo => qo.Question)
                .HasForeignKey(qo => qo.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Quiz>()
                .HasMany(q => q.Attempts)
                .WithOne(qa => qa.Quiz)
                .HasForeignKey(qa => qa.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuizAttempt>()
                .HasOne(qa => qa.Student)
                .WithMany()
                .HasForeignKey(qa => qa.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<QuizAttempt>()
                .HasMany(qa => qa.Answers)
                .WithOne(ans => ans.Attempt)
                .HasForeignKey(ans => ans.AttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuizAnswer>()
                .HasOne(ans => ans.Question)
                .WithMany()
                .HasForeignKey(ans => ans.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<QuizAnswer>()
                .HasOne(ans => ans.SelectedOption)
                .WithMany()
                .HasForeignKey(ans => ans.SelectedOptionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Assignments Relationships
            modelBuilder.Entity<Course>()
                .HasMany(c => c.Assignments)
                .WithOne(a => a.Course)
                .HasForeignKey(a => a.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Section>()
                .HasMany(s => s.Assignments)
                .WithOne(a => a.Section)
                .HasForeignKey(a => a.SectionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Assignment>()
                .HasMany(a => a.Submissions)
                .WithOne(sub => sub.Assignment)
                .HasForeignKey(sub => sub.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AssignmentSubmission>()
                .HasOne(sub => sub.Student)
                .WithMany()
                .HasForeignKey(sub => sub.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Q&A Relationships
            modelBuilder.Entity<Lesson>()
                .HasMany(l => l.Questions)
                .WithOne(q => q.Lesson)
                .HasForeignKey(q => q.LessonId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LessonQuestion>()
                .HasOne(q => q.Student)
                .WithMany()
                .HasForeignKey(q => q.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LessonQuestion>()
                .HasMany(q => q.Replies)
                .WithOne(r => r.Question)
                .HasForeignKey(r => r.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LessonReply>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Reviews Relationships
            modelBuilder.Entity<Course>()
                .HasMany(c => c.Reviews)
                .WithOne(r => r.Course)
                .HasForeignKey(r => r.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CourseReview>()
                .HasOne(r => r.Student)
                .WithMany()
                .HasForeignKey(r => r.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Gen Alpha: Certificates Relationships
            modelBuilder.Entity<Certificate>()
                .HasOne(cert => cert.Student)
                .WithMany()
                .HasForeignKey(cert => cert.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Certificate>()
                .HasOne(cert => cert.Course)
                .WithMany()
                .HasForeignKey(cert => cert.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Gamification Relationships
            modelBuilder.Entity<StudentBadge>()
                .HasOne(sb => sb.Student)
                .WithMany()
                .HasForeignKey(sb => sb.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudentBadge>()
                .HasOne(sb => sb.Badge)
                .WithMany(b => b.StudentBadges)
                .HasForeignKey(sb => sb.BadgeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Notification Relationships
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Course Payment Relationships
            modelBuilder.Entity<CoursePayment>()
                .HasOne(cp => cp.Student)
                .WithMany()
                .HasForeignKey(cp => cp.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CoursePayment>()
                .HasOne(cp => cp.Course)
                .WithMany()
                .HasForeignKey(cp => cp.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CoursePayment>()
                .HasOne(cp => cp.Coupon)
                .WithMany()
                .HasForeignKey(cp => cp.CouponId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}