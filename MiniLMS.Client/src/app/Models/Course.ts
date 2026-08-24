export interface Lesson {
  id: number;
  courseId?: number;
  sectionId?: number;
  title: string;
  content: string;
  lessonType?: 'Video' | 'Article' | 'Resource';
  videoUrl?: string;
  durationMinutes?: number;
  isFreePreview?: boolean;
  resourceUrl?: string;
  orderIndex: number;
}

export interface Section {
  id: number;
  courseId: number;
  title: string;
  orderIndex: number;
  lessons: Lesson[];
  quizzes?: QuizDto[];
  assignments?: AssignmentDto[];
}

export interface Course {
  id: number;
  instructorId?: number;
  instructorName?: string;
  title: string;
  description: string;
  category: string;
  isPublished: boolean;
  approvalStatus?: 'Draft' | 'PendingReview' | 'Approved' | 'Rejected';
  rejectionReason?: string;
  averageRating?: number;
  reviewsCount?: number;
  sections?: Section[];
  lessons?: Lesson[];
}

export interface StudentLessonDto {
  lessonId: number;
  sectionId?: number;
  lessonTitle: string;
  lessonType: 'Video' | 'Article' | 'Resource';
  videoUrl?: string;
  durationMinutes: number;
  isFreePreview: boolean;
  resourceUrl?: string;
  content?: string;
  orderIndex: number;
  isCompleted: boolean;
  completedDate?: string;
  lastWatchedSeconds?: number;
  watchPercentage?: number;
}

export interface StudentSectionDto {
  sectionId: number;
  title: string;
  orderIndex: number;
  lessons: StudentLessonDto[];
}

export interface CourseDetailsDto {
  courseId: number;
  instructorId?: number;
  instructorName?: string;
  title: string;
  description: string;
  category: string;
  isEnrolled: boolean;
  progressPercentage: number;
  status: string;
  averageRating: number;
  reviewsCount: number;
  totalDurationMinutes: number;
  totalLessonsCount: number;
  completedLessonsCount: number;
  quizzesCount: number;
  assignmentsCount: number;
  sections: StudentSectionDto[];
  lessons: StudentLessonDto[];
}

export interface CreateSectionDto {
  title: string;
  orderIndex?: number;
}

export interface CreateLessonDto {
  sectionId?: number;
  courseId?: number;
  title: string;
  content: string;
  lessonType: string;
  videoUrl?: string;
  durationMinutes: number;
  isFreePreview: boolean;
  resourceUrl?: string;
  orderIndex?: number;
}

export interface InstructorProfileDto {
  id: number;
  fullName: string;
  email: string;
  headline?: string;
  bio?: string;
  profilePictureUrl?: string;
  websiteUrl?: string;
  linkedInUrl?: string;
  githubUrl?: string;
  youTubeUrl?: string;
  totalCourses: number;
  totalStudents: number;
  courses: Course[];
}

export interface UpdateInstructorProfileDto {
  fullName: string;
  headline?: string;
  bio?: string;
  profilePictureUrl?: string;
  websiteUrl?: string;
  linkedInUrl?: string;
  githubUrl?: string;
  youTubeUrl?: string;
}

export interface InstructorStudentDto {
  studentId: number;
  fullName: string;
  email: string;
  courseId: number;
  courseTitle: string;
  enrollmentDate: string;
  progressPercentage: number;
  status: string;
}

// ================= QUIZZES =================

export interface QuizOptionDto {
  id: number;
  questionId: number;
  optionText: string;
  isCorrect?: boolean;
}

export interface CreateQuizOptionDto {
  optionText: string;
  isCorrect: boolean;
}

export interface QuizQuestionDto {
  id: number;
  quizId: number;
  questionText: string;
  questionType: 'MCQ' | 'TrueFalse' | 'ShortAnswer';
  points: number;
  explanation?: string;
  orderIndex: number;
  options: QuizOptionDto[];
}

export interface CreateQuizQuestionDto {
  questionText: string;
  questionType: 'MCQ' | 'TrueFalse' | 'ShortAnswer';
  points: number;
  explanation?: string;
  orderIndex?: number;
  options: CreateQuizOptionDto[];
}

export interface QuizDto {
  id: number;
  courseId: number;
  sectionId?: number;
  title: string;
  description?: string;
  passingScorePercentage: number;
  timeLimitMinutes?: number;
  orderIndex: number;
  totalQuestions: number;
  totalPoints: number;
  isPassedByStudent?: boolean;
  bestScorePercentage?: number;
  questions: QuizQuestionDto[];
}

export interface CreateQuizDto {
  sectionId?: number;
  title: string;
  description?: string;
  passingScorePercentage: number;
  timeLimitMinutes?: number;
  orderIndex?: number;
  questions: CreateQuizQuestionDto[];
}

export interface SubmitQuizAnswerDto {
  questionId: number;
  selectedOptionId?: number;
  answerText?: string;
}

export interface SubmitQuizDto {
  answers: SubmitQuizAnswerDto[];
}

export interface QuizQuestionResultDto {
  questionId: number;
  questionText: string;
  isCorrect: boolean;
  selectedOptionId?: number;
  correctOptionId?: number;
  explanation?: string;
}

export interface QuizResultDto {
  attemptId: number;
  quizId: number;
  score: number;
  totalPoints: number;
  percentage: number;
  isPassed: boolean;
  attemptDate: string;
  questionsResults: QuizQuestionResultDto[];
}

// ================= ASSIGNMENTS =================

export interface AssignmentSubmissionDto {
  id: number;
  assignmentId: number;
  studentId: number;
  studentName: string;
  studentEmail: string;
  submissionDate: string;
  fileUrl?: string;
  studentNotes?: string;
  grade?: number;
  instructorFeedback?: string;
  status: 'Submitted' | 'Graded' | 'ResubmissionRequested';
}

export interface AssignmentDto {
  id: number;
  courseId: number;
  sectionId?: number;
  title: string;
  description: string;
  attachmentUrl?: string;
  maxScore: number;
  dueDate?: string;
  orderIndex: number;
  isSubmittedByStudent: boolean;
  mySubmission?: AssignmentSubmissionDto;
  totalSubmissionsCount: number;
}

export interface CreateAssignmentDto {
  sectionId?: number;
  title: string;
  description: string;
  attachmentUrl?: string;
  maxScore: number;
  dueDate?: string;
  orderIndex?: number;
}

export interface SubmitAssignmentDto {
  fileUrl?: string;
  studentNotes?: string;
}

export interface GradeAssignmentDto {
  grade: number;
  instructorFeedback?: string;
}

// ================= Q&A =================

export interface LessonReplyDto {
  id: number;
  questionId: number;
  userId: number;
  authorName: string;
  authorRole: string;
  content: string;
  createdAt: string;
  isInstructorReply: boolean;
  isAcceptedAnswer: boolean;
  upvotesCount: number;
}

export interface CreateLessonReplyDto {
  content: string;
}

export interface LessonQuestionDto {
  id: number;
  lessonId: number;
  lessonTitle: string;
  studentId: number;
  studentName: string;
  title: string;
  content: string;
  videoTimestampSeconds?: number;
  createdAt: string;
  isResolved: boolean;
  upvotesCount: number;
  repliesCount: number;
  replies: LessonReplyDto[];
}

export interface CreateLessonQuestionDto {
  title: string;
  content: string;
  videoTimestampSeconds?: number;
}

// ================= REVIEWS =================

export interface CourseReviewDto {
  id: number;
  courseId: number;
  studentId: number;
  studentName: string;
  rating: number;
  comment?: string;
  createdAt: string;
  isApproved: boolean;
}

export interface CreateCourseReviewDto {
  rating: number;
  comment?: string;
}

export interface CourseRatingSummaryDto {
  averageRating: number;
  totalReviews: number;
  fiveStarCount: number;
  fourStarCount: number;
  threeStarCount: number;
  twoStarCount: number;
  oneStarCount: number;
}