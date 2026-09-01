import { Component, Input, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AiTutorService } from '../../../Core/Services/ai-tutor.service';
import { AiPracticeQuestionDto } from '../../../Models/GenAlpha';
import { StudentLessonDto } from '../../../Models/Course';

type AiTab = 'explain' | 'summary' | 'practice';

@Component({
  selector: 'app-ai-tutor-widget',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './ai-tutor-widget.component.html',
  styleUrl: './ai-tutor-widget.component.css'
})
export class AiTutorWidgetComponent implements OnInit {
  @Input() lesson: StudentLessonDto | null = null;
  @Input() courseTitle: string = '';

  readonly isOpen = signal(false);
  readonly activeTab = signal<AiTab>('explain');
  readonly loading = signal(false);

  // Explain Concept
  userPrompt = '';
  readonly explanationResult = signal<string | null>(null);

  // Summarize
  readonly summaryResult = signal<string | null>(null);

  // Practice Questions
  readonly practiceQuestions = signal<AiPracticeQuestionDto[]>([]);
  selectedAnswers: { [questionId: number]: number } = {};
  showFeedback: { [questionId: number]: boolean } = {};

  constructor(private aiTutorService: AiTutorService) {}

  ngOnInit(): void {}

  toggleOpen(): void {
    this.isOpen.update(v => !v);
  }

  setTab(tab: AiTab): void {
    this.activeTab.set(tab);
    if (tab === 'summary' && !this.summaryResult() && this.lesson) {
      this.generateSummary();
    } else if (tab === 'practice' && this.practiceQuestions().length === 0 && this.lesson) {
      this.generateQuestions();
    }
  }

  askExplain(): void {
    if (!this.userPrompt.trim()) return;

    this.loading.set(true);
    this.explanationResult.set(null);

    this.aiTutorService.explainConcept({
      prompt: this.userPrompt,
      lessonTitle: this.lesson?.lessonTitle || this.courseTitle,
      lessonContext: this.lesson?.content || '',
      language: 'Arabic'
    }).subscribe({
      next: (res) => {
        this.explanationResult.set(res.output);
        this.loading.set(false);
      },
      error: () => {
        this.explanationResult.set('عذراً، حدث خطأ أثناء التواصل مع المعلم الذكي. يرجى المحاولة مرة أخرى.');
        this.loading.set(false);
      }
    });
  }

  generateSummary(): void {
    if (!this.lesson) return;

    this.loading.set(true);
    this.summaryResult.set(null);

    this.aiTutorService.summarizeLesson({
      lessonTitle: this.lesson.lessonTitle,
      lessonContent: this.lesson.content || '',
      language: 'Arabic'
    }).subscribe({
      next: (res) => {
        this.summaryResult.set(res.output);
        this.loading.set(false);
      },
      error: () => {
        this.summaryResult.set('تعذر توليد الملخص حالياً.');
        this.loading.set(false);
      }
    });
  }

  generateQuestions(): void {
    if (!this.lesson) return;

    this.loading.set(true);
    this.practiceQuestions.set([]);
    this.selectedAnswers = {};
    this.showFeedback = {};

    this.aiTutorService.generatePracticeQuestions({
      topic: this.lesson.lessonTitle,
      lessonTitle: this.lesson.lessonTitle,
      questionCount: 3,
      difficulty: 'Medium'
    }).subscribe({
      next: (res) => {
        this.practiceQuestions.set(res);
        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
      }
    });
  }

  selectOption(questionId: number, optionIndex: number): void {
    this.selectedAnswers[questionId] = optionIndex;
    this.showFeedback[questionId] = true;
  }
}
