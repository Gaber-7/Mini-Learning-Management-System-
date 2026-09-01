import { Component, OnInit, ElementRef, ViewChild, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { CourseService } from '../../../Core/Services/course.service';
import { QuizService } from '../../../Core/Services/quiz.service';
import { AssignmentService } from '../../../Core/Services/assignment.service';
import { QnAService } from '../../../Core/Services/qna.service';
import { ReviewService } from '../../../Core/Services/review.service';
import { CertificateService } from '../../../Core/Services/certificate.service';
import { GamificationService } from '../../../Core/Services/gamification.service';
import { AiTutorWidgetComponent } from '../ai-tutor-widget/ai-tutor-widget.component';
import { NotificationBellComponent } from '../../../Shared/notification-bell/notification-bell.component';
import { GamificationWidgetComponent } from '../../../Shared/gamification-widget/gamification-widget.component';
import {
  AssignmentDto,
  CourseDetailsDto,
  CourseRatingSummaryDto,
  CourseReviewDto,
  CreateCourseReviewDto,
  CreateLessonQuestionDto,
  LessonQuestionDto,
  LessonReplyDto,
  QuizDto,
  QuizResultDto,
  StudentLessonDto,
  SubmitAssignmentDto,
  SubmitQuizAnswerDto
} from '../../../Models/Course';

type ClassroomTab = 'content' | 'qna' | 'quizzes' | 'assignments' | 'reviews';

@Component({
  selector: 'app-course-classroom',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    AiTutorWidgetComponent,
    NotificationBellComponent,
    GamificationWidgetComponent
  ],
  templateUrl: './course-classroom.component.html',
  styleUrl: './course-classroom.component.css'
})
export class CourseClassroomComponent implements OnInit {
  @ViewChild('htmlVideoPlayer') htmlVideoPlayer?: ElementRef<HTMLVideoElement>;

  courseId!: number;
  readonly course = signal<CourseDetailsDto | null>(null);
  readonly activeLesson = signal<StudentLessonDto | null>(null);
  readonly loading = signal(true);
  readonly activeTab = signal<ClassroomTab>('content');

  // Video & Playback Resume
  currentVideoCurrentTime = 0;
  currentVideoDuration = 0;
  videoWatchPercentage = 0;
  private progressSaveTimeout: any = null;

  // Q&A
  readonly questions = signal<LessonQuestionDto[]>([]);
  readonly loadingQuestions = signal(false);
  newQuestionTitle = '';
  newQuestionContent = '';
  includeTimestampInQuestion = true;
  replyInputs: { [questionId: number]: string } = {};

  // Quizzes
  readonly courseQuizzes = signal<QuizDto[]>([]);
  readonly activeQuiz = signal<QuizDto | null>(null);
  readonly quizAnswers = signal<{ [questionId: number]: { selectedOptionId?: number; answerText?: string } }>({});
  readonly quizResult = signal<QuizResultDto | null>(null);
  readonly isSubmittingQuiz = signal(false);

  // Assignments
  readonly courseAssignments = signal<AssignmentDto[]>([]);
  readonly activeAssignment = signal<AssignmentDto | null>(null);
  submissionFileUrl = '';
  submissionNotes = '';
  readonly isSubmittingAssignment = signal(false);

  // Reviews
  readonly reviews = signal<CourseReviewDto[]>([]);
  readonly ratingSummary = signal<CourseRatingSummaryDto | null>(null);
  myRating = 5;
  myReviewComment = '';
  readonly isSubmittingReview = signal(false);

  toastMessage = '';
  toastType: 'success' | 'error' = 'success';
  private toastTimer: any = null;

  // Certificates & Gamification
  readonly earnedCertificateCode = signal<string | null>(null);
  readonly isClaimingCertificate = signal(false);

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private courseService: CourseService,
    private quizService: QuizService,
    private assignmentService: AssignmentService,
    private qnaService: QnAService,
    private reviewService: ReviewService,
    private certificateService: CertificateService,
    private gamificationService: GamificationService,
    private sanitizer: DomSanitizer
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.courseId = Number(idParam);
      this.loadCourseData(this.courseId);
      this.checkExistingCertificate(this.courseId);
    } else {
      this.router.navigate(['/student/dashboard']);
    }
  }

  checkExistingCertificate(courseId: number): void {
    this.certificateService.getCertificateForCourse(courseId).subscribe({
      next: (cert) => {
        if (cert && cert.certificateCode) {
          this.earnedCertificateCode.set(cert.certificateCode);
        }
      },
      error: () => {}
    });
  }

  claimCertificate(): void {
    this.isClaimingCertificate.set(true);
    this.certificateService.issueCertificate(this.courseId).subscribe({
      next: (cert) => {
        this.isClaimingCertificate.set(false);
        this.earnedCertificateCode.set(cert.certificateCode);
        this.showToast('success', '🏆 مبارك! تم إصدار شهادتك الرقمية المعتمدة بنجاح!');
        // Award XP bonus
        this.gamificationService.awardXP(250, 'Course Completed & Certificate Earned').subscribe();
      },
      error: (err) => {
        this.isClaimingCertificate.set(false);
        this.showToast('error', 'تعذر إصدار الشهادة: ' + (err.error?.message || err.message));
      }
    });
  }

  loadCourseData(courseId: number): void {
    this.loading.set(true);
    this.courseService.getCourseDetails(courseId).subscribe({
      next: (data) => {
        this.course.set(data);
        this.loading.set(false);

        // Select first incomplete lesson or first lesson
        const firstIncomplete = data.lessons?.find((l) => !l.isCompleted);
        const initial = firstIncomplete || data.lessons?.[0] || null;
        if (initial) {
          this.selectLesson(initial);
        }

        // Load modules data
        this.loadCourseQuizzes(courseId);
        this.loadCourseAssignments(courseId);
        this.loadCourseReviews(courseId);
      },
      error: () => {
        this.loading.set(false);
        this.showToast('error', 'Failed to load course details.');
      }
    });
  }

  setTab(tab: ClassroomTab): void {
    this.activeTab.set(tab);
  }

  /* ------------------------------------------------------------------ *
   *  Video & Lesson Player
   * ------------------------------------------------------------------ */
  selectLesson(lesson: StudentLessonDto): void {
    this.activeLesson.set(lesson);
    this.videoWatchPercentage = lesson.watchPercentage || 0;
    this.currentVideoCurrentTime = lesson.lastWatchedSeconds || 0;

    this.loadLessonQuestions(lesson.lessonId);

    // Apply resume playback
    setTimeout(() => {
      if (this.htmlVideoPlayer?.nativeElement) {
        const vid = this.htmlVideoPlayer.nativeElement;
        if (lesson.lastWatchedSeconds && lesson.lastWatchedSeconds > 0) {
          vid.currentTime = lesson.lastWatchedSeconds;
        }
      }
    }, 250);
  }

  onVideoTimeUpdate(event: Event): void {
    const video = event.target as HTMLVideoElement;
    if (!video || !video.duration) return;

    this.currentVideoCurrentTime = Math.floor(video.currentTime);
    this.currentVideoDuration = Math.floor(video.duration);

    const percentage = Math.min(100, Math.round((this.currentVideoCurrentTime / this.currentVideoDuration) * 100));
    this.videoWatchPercentage = percentage;

    if (this.progressSaveTimeout) clearTimeout(this.progressSaveTimeout);
    this.progressSaveTimeout = setTimeout(() => {
      this.syncWatchProgress(this.currentVideoCurrentTime, percentage);
    }, 3000);
  }

  onVideoEnded(): void {
    this.syncWatchProgress(this.currentVideoDuration, 100, true);
  }

  private syncWatchProgress(seconds: number, percentage: number, forceCompleted: boolean = false): void {
    const c = this.course();
    const l = this.activeLesson();
    if (!c || !l) return;

    this.courseService.updateWatchProgress(c.courseId, l.lessonId, seconds, percentage, forceCompleted).subscribe({
      next: (updated) => {
        if (updated.isCompleted && !l.isCompleted) {
          l.isCompleted = true;
          this.showToast('success', `🎉 Lesson completed: ${l.lessonTitle}`);
          this.updateCourseStateAfterCompletion(l.lessonId);
        }
      }
    });
  }

  toggleLesson(lesson: StudentLessonDto): void {
    const c = this.course();
    if (!c) return;

    this.courseService.toggleLessonCompletion(c.courseId, lesson.lessonId).subscribe({
      next: () => {
        lesson.isCompleted = !lesson.isCompleted;
        this.updateCourseStateAfterCompletion(lesson.lessonId);
        this.showToast(
          'success',
          lesson.isCompleted ? `Marked "${lesson.lessonTitle}" as completed.` : `Marked "${lesson.lessonTitle}" as incomplete.`
        );
      },
      error: () => this.showToast('error', 'Failed to update lesson status.')
    });
  }

  private updateCourseStateAfterCompletion(lessonId: number): void {
    const c = this.course();
    if (!c) return;

    const completed = c.lessons?.filter((l) => l.isCompleted).length ?? 0;
    const total = c.totalLessonsCount || c.lessons?.length || 1;
    const pct = Math.round((completed / total) * 100);

    c.progressPercentage = pct;
    c.completedLessonsCount = completed;
    c.status = pct === 100 ? 'Completed' : 'InProgress';
  }

  seekVideoToSeconds(seconds?: number): void {
    if (seconds === undefined || seconds === null) return;
    if (this.htmlVideoPlayer?.nativeElement) {
      this.htmlVideoPlayer.nativeElement.currentTime = seconds;
      this.htmlVideoPlayer.nativeElement.play();
    }
  }

  formatSeconds(totalSeconds?: number): string {
    if (!totalSeconds) return '00:00';
    const m = Math.floor(totalSeconds / 60);
    const s = Math.floor(totalSeconds % 60);
    return `${m < 10 ? '0' : ''}${m}:${s < 10 ? '0' : ''}${s}`;
  }

  getSafeVideoUrl(url?: string): SafeResourceUrl | null {
    if (!url) return null;
    let embedUrl = url;
    if (url.includes('youtube.com/watch?v=')) {
      embedUrl = url.replace('watch?v=', 'embed/');
    } else if (url.includes('youtu.be/')) {
      embedUrl = url.replace('youtu.be/', 'youtube.com/embed/');
    }
    return this.sanitizer.bypassSecurityTrustResourceUrl(embedUrl);
  }

  /* ------------------------------------------------------------------ *
   *  Q&A
   * ------------------------------------------------------------------ */
  loadLessonQuestions(lessonId: number): void {
    this.loadingQuestions.set(true);
    this.qnaService.getLessonQuestions(lessonId).subscribe({
      next: (data) => {
        this.questions.set(data);
        this.loadingQuestions.set(false);
      },
      error: () => this.loadingQuestions.set(false)
    });
  }

  submitQuestion(): void {
    const l = this.activeLesson();
    if (!l || !this.newQuestionTitle.trim() || !this.newQuestionContent.trim()) return;

    const dto: CreateLessonQuestionDto = {
      title: this.newQuestionTitle.trim(),
      content: this.newQuestionContent.trim(),
      videoTimestampSeconds: this.includeTimestampInQuestion ? this.currentVideoCurrentTime : undefined
    };

    this.qnaService.askQuestion(l.lessonId, dto).subscribe({
      next: (q) => {
        this.questions.update((list) => [q, ...list]);
        this.newQuestionTitle = '';
        this.newQuestionContent = '';
        this.showToast('success', 'Question posted to the discussion.');
      },
      error: () => this.showToast('error', 'Failed to post question.')
    });
  }

  submitReply(questionId: number): void {
    const text = this.replyInputs[questionId]?.trim();
    if (!text) return;

    this.qnaService.addReply(questionId, { content: text }).subscribe({
      next: (reply) => {
        this.questions.update((list) =>
          list.map((q) => (q.id === questionId ? { ...q, replies: [...q.replies, reply], repliesCount: q.repliesCount + 1 } : q))
        );
        this.replyInputs[questionId] = '';
        this.showToast('success', 'Reply added!');
      },
      error: () => this.showToast('error', 'Failed to post reply.')
    });
  }

  upvoteQuestion(q: LessonQuestionDto): void {
    this.qnaService.upvoteQuestion(q.id).subscribe({
      next: (res) => (q.upvotesCount = res.upvotesCount)
    });
  }

  upvoteReply(r: LessonReplyDto): void {
    this.qnaService.upvoteReply(r.id).subscribe({
      next: (res) => (r.upvotesCount = res.upvotesCount)
    });
  }

  markBestAnswer(reply: LessonReplyDto, question: LessonQuestionDto): void {
    this.qnaService.markAcceptedAnswer(reply.id).subscribe({
      next: () => {
        question.replies.forEach((r) => (r.isAcceptedAnswer = r.id === reply.id));
        question.isResolved = true;
        this.showToast('success', 'Marked as accepted best answer.');
      }
    });
  }

  /* ------------------------------------------------------------------ *
   *  Quizzes
   * ------------------------------------------------------------------ */
  loadCourseQuizzes(courseId: number): void {
    this.quizService.getCourseQuizzes(courseId).subscribe({
      next: (quizzes) => this.courseQuizzes.set(quizzes)
    });
  }

  startQuiz(quiz: QuizDto): void {
    this.activeQuiz.set(quiz);
    this.quizResult.set(null);
    this.quizAnswers.set({});
    this.setTab('quizzes');
  }

  selectQuizOption(questionId: number, optionId: number): void {
    this.quizAnswers.update((map) => ({
      ...map,
      [questionId]: { selectedOptionId: optionId }
    }));
  }

  submitQuizAttempt(): void {
    const quiz = this.activeQuiz();
    if (!quiz) return;

    const answersList: SubmitQuizAnswerDto[] = Object.entries(this.quizAnswers()).map(([qId, ans]) => ({
      questionId: Number(qId),
      selectedOptionId: ans.selectedOptionId,
      answerText: ans.answerText
    }));

    this.isSubmittingQuiz.set(true);
    this.quizService.submitQuiz(quiz.id, { answers: answersList }).subscribe({
      next: (result) => {
        this.isSubmittingQuiz.set(false);
        this.quizResult.set(result);
        if (result.isPassed) {
          this.showToast('success', `🎉 Congratulations! You passed with ${result.percentage}%`);
        } else {
          this.showToast('error', `You scored ${result.percentage}%. Passing score is ${quiz.passingScorePercentage}%.`);
        }
        this.loadCourseQuizzes(this.courseId);
      },
      error: () => {
        this.isSubmittingQuiz.set(false);
        this.showToast('error', 'Failed to submit quiz attempt.');
      }
    });
  }

  closeQuizAttempt(): void {
    this.activeQuiz.set(null);
    this.quizResult.set(null);
  }

  /* ------------------------------------------------------------------ *
   *  Assignments
   * ------------------------------------------------------------------ */
  loadCourseAssignments(courseId: number): void {
    this.assignmentService.getCourseAssignments(courseId).subscribe({
      next: (assignments) => this.courseAssignments.set(assignments)
    });
  }

  openAssignment(assignment: AssignmentDto): void {
    this.activeAssignment.set(assignment);
    this.submissionFileUrl = assignment.mySubmission?.fileUrl || '';
    this.submissionNotes = assignment.mySubmission?.studentNotes || '';
    this.setTab('assignments');
  }

  submitAssignment(): void {
    const assignment = this.activeAssignment();
    if (!assignment) return;

    if (!this.submissionFileUrl.trim() && !this.submissionNotes.trim()) {
      this.showToast('error', 'Please provide a project link/file URL or notes.');
      return;
    }

    this.isSubmittingAssignment.set(true);
    const dto: SubmitAssignmentDto = {
      fileUrl: this.submissionFileUrl.trim(),
      studentNotes: this.submissionNotes.trim()
    };

    this.assignmentService.submitAssignment(assignment.id, dto).subscribe({
      next: (submission) => {
        this.isSubmittingAssignment.set(false);
        assignment.isSubmittedByStudent = true;
        assignment.mySubmission = submission;
        this.showToast('success', 'Assignment submitted successfully for instructor review!');
      },
      error: () => {
        this.isSubmittingAssignment.set(false);
        this.showToast('error', 'Failed to submit assignment.');
      }
    });
  }

  /* ------------------------------------------------------------------ *
   *  Reviews
   * ------------------------------------------------------------------ */
  loadCourseReviews(courseId: number): void {
    this.reviewService.getCourseReviews(courseId).subscribe({
      next: (reviews) => this.reviews.set(reviews)
    });
    this.reviewService.getRatingSummary(courseId).subscribe({
      next: (summary) => this.ratingSummary.set(summary)
    });
  }

  submitReview(): void {
    if (!this.myReviewComment.trim()) {
      this.showToast('error', 'Please write your review comment.');
      return;
    }

    this.isSubmittingReview.set(true);
    const dto: CreateCourseReviewDto = {
      rating: this.myRating,
      comment: this.myReviewComment.trim()
    };

    this.reviewService.addOrUpdateReview(this.courseId, dto).subscribe({
      next: () => {
        this.isSubmittingReview.set(false);
        this.showToast('success', 'Thank you! Your review has been submitted.');
        this.myReviewComment = '';
        this.loadCourseReviews(this.courseId);
      },
      error: () => {
        this.isSubmittingReview.set(false);
        this.showToast('error', 'Failed to submit review.');
      }
    });
  }

  setRating(stars: number): void {
    this.myRating = stars;
  }

  showToast(type: 'success' | 'error', message: string): void {
    if (this.toastTimer) clearTimeout(this.toastTimer);
    this.toastType = type;
    this.toastMessage = message;
    this.toastTimer = setTimeout(() => {
      this.toastMessage = '';
    }, 4000);
  }
}
