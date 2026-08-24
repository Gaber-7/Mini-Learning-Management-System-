using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MiniLMS.Data.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace MiniLMS.Data.Data
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

            // Instructor Courses (1-to-Many)
            modelBuilder.Entity<Instructor>()
                .HasMany(i => i.Courses)
                .WithOne(c => c.Instructor)
                .HasForeignKey(c => c.InstructorId)
                .OnDelete(DeleteBehavior.SetNull);

            // Enrollment Unique constraint
            modelBuilder.Entity<Enrollment>()
                .HasIndex(e => new { e.StudentId, e.CourseId })
                .IsUnique();

            // Course Sections (Cascade delete)
            modelBuilder.Entity<Course>()
                .HasMany(c => c.Sections)
                .WithOne(s => s.Course)
                .HasForeignKey(s => s.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Course Lessons (Cascade delete)
            modelBuilder.Entity<Course>()
                .HasMany(c => c.Lessons)
                .WithOne(l => l.Course)
                .HasForeignKey(l => l.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            // Section Lessons (Restrict to avoid multiple cascade paths)
            modelBuilder.Entity<Section>()
                .HasMany(s => s.Lessons)
                .WithOne(l => l.Section)
                .HasForeignKey(l => l.SectionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Progress Enrollments & Lessons
            modelBuilder.Entity<LessonProgress>()
                .HasOne(lp => lp.Enrollment)
                .WithMany(e => e.LessonProgresses)
                .HasForeignKey(lp => lp.EnrollmentId)
                .OnDelete(DeleteBehavior.Restrict);

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

            modelBuilder.Entity<QuizAttempt>()
                .HasOne(qa => qa.Student)
                .WithMany()
                .HasForeignKey(qa => qa.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<QuizAttempt>()
                .HasMany(qa => qa.Answers)
                .WithOne(a => a.Attempt)
                .HasForeignKey(a => a.AttemptId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuizAnswer>()
                .HasOne(a => a.Question)
                .WithMany(q => q.Answers)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<QuizAnswer>()
                .HasOne(a => a.SelectedOption)
                .WithMany()
                .HasForeignKey(a => a.SelectedOptionId)
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
        }
    }
}