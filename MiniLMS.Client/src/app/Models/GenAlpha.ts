// Certificate Models
export interface CertificateDto {
  id: number;
  certificateCode: string;
  studentId: number;
  studentName: string;
  courseId: number;
  courseTitle: string;
  instructorName?: string;
  issueDate: string;
  finalScorePercentage?: number;
  qrVerificationUrl: string;
  linkedInShareUrl: string;
}

export interface CertificateVerificationResultDto {
  isValid: boolean;
  certificateCode: string;
  studentName: string;
  courseTitle: string;
  instructorName: string;
  issueDate: string;
  finalScorePercentage: number;
  issuer: string;
  status: string;
}

// AI Tutor Models
export interface AiExplainRequestDto {
  prompt: string;
  lessonTitle?: string;
  lessonContext?: string;
  language?: string;
}

export interface AiSummarizeRequestDto {
  lessonTitle: string;
  lessonContent?: string;
  language?: string;
}

export interface AiPracticeQuestionsRequestDto {
  topic: string;
  lessonTitle?: string;
  questionCount?: number;
  difficulty?: string;
}

export interface AiResponseDto {
  success: boolean;
  output: string;
  modelUsed?: string;
  timestamp: string;
}

export interface AiPracticeQuestionDto {
  id: number;
  questionText: string;
  options: string[];
  correctOptionIndex: number;
  explanation: string;
}

// Gamification Models
export interface BadgeDto {
  id: number;
  code: string;
  title: string;
  description: string;
  iconUrl: string;
  xpReward: number;
  isEarned: boolean;
  earnedDate?: string;
}

export interface StudentGamificationDto {
  studentId: number;
  studentName: string;
  totalXP: number;
  level: number;
  levelTitle: string;
  currentStreakDays: number;
  longestStreakDays: number;
  lastActiveDate: string;
  nextLevelXP: number;
  currentLevelProgressXP: number;
  progressToNextLevelPercentage: number;
  earnedBadges: BadgeDto[];
}

export interface LeaderboardItemDto {
  rank: number;
  studentId: number;
  studentName: string;
  totalXP: number;
  level: number;
  streakDays: number;
  badgesCount: number;
}

// Real-Time Notification Models
export interface NotificationDto {
  id: number;
  userId: number;
  title: string;
  message: string;
  notificationType: string;
  actionUrl?: string;
  isRead: boolean;
  createdAt: string;
}

// Monetization & PayPal Models
export interface CouponResultDto {
  isValid: boolean;
  code: string;
  originalPrice: number;
  discountPercentage: number;
  discountAmount: number;
  finalPrice: number;
  message: string;
}

export interface CreatePaymentOrderDto {
  courseId: number;
  couponCode?: string;
  paymentMethod?: string;
}

export interface PaymentOrderResultDto {
  orderId: string;
  courseId: number;
  courseTitle: string;
  originalPrice: number;
  finalAmount: number;
  currency: string;
  approvalUrl: string;
  status: string;
}

export interface CapturePaymentDto {
  orderId: string;
  courseId: number;
  couponCode?: string;
  transactionId?: string;
  paymentMethod?: string;
}

export interface PaymentResultDto {
  success: boolean;
  paymentId: number;
  courseId: number;
  transactionId: string;
  amountPaid: number;
  message: string;
  enrolled: boolean;
}

export interface InstructorWalletDto {
  instructorId: number;
  instructorName: string;
  totalEarnings: number;
  availableBalance: number;
  withdrawnAmount: number;
  commissionPercentage: number;
  lastUpdated: string;
  recentPayments: {
    paymentId: number;
    courseTitle: string;
    studentName: string;
    amount: number;
    instructorShare: number;
    paymentDate: string;
    status: string;
  }[];
}
