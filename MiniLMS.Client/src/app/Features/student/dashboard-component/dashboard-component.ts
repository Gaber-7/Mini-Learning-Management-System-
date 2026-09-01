import { Component, OnInit, computed, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { CourseService } from '../../../Core/Services/course.service';
import { CourseDetailsDto } from '../../../Models/Course';
import { GamificationWidgetComponent } from '../../../Shared/gamification-widget/gamification-widget.component';
import { NotificationBellComponent } from '../../../Shared/notification-bell/notification-bell.component';

type CourseFilter = 'all' | 'in-progress' | 'completed' | 'not-started';
type CourseSort = 'title' | 'progress-desc' | 'progress-asc';

@Component({
  selector: 'app-student-dashboard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, GamificationWidgetComponent, NotificationBellComponent],
  templateUrl: './dashboard-component.html',
  styleUrl: './dashboard-component.css',
})
export class DashboardComponent implements OnInit {
  readonly courses = signal<CourseDetailsDto[]>([]);
  readonly loading = signal(false);

  readonly searchTerm = signal('');
  readonly activeFilter = signal<CourseFilter>('all');
  readonly sortBy = signal<CourseSort>('progress-desc');

  readonly successMessage = signal('');
  readonly errorMessage = signal('');
  readonly userName = signal('Student');

  readonly skeletons = [1, 2, 3, 4, 5, 6];

  constructor(
    private courseService: CourseService,
    private router: Router
  ) {}

  readonly greeting = computed(() => {
    const hour = new Date().getHours();
    if (hour < 12) return 'Good morning';
    if (hour < 18) return 'Good afternoon';
    return 'Good evening';
  });

  readonly stats = computed(() => {
    const list = this.courses();
    const total = list.length;
    const completed = list.filter((c) => c.progressPercentage === 100).length;
    const notStarted = list.filter((c) => !c.progressPercentage).length;
    const inProgress = total - completed - notStarted;

    const totalLessons = list.reduce((sum, c) => sum + (c.totalLessonsCount || c.lessons?.length || 0), 0);
    const completedLessons = list.reduce(
      (sum, c) => sum + (c.completedLessonsCount || c.lessons?.filter((l) => l.isCompleted).length || 0),
      0
    );

    const overall = total
      ? Math.round(list.reduce((sum, c) => sum + (c.progressPercentage ?? 0), 0) / total)
      : 0;

    return { total, completed, inProgress, notStarted, totalLessons, completedLessons, overall };
  });

  readonly filteredCourses = computed(() => {
    const term = this.searchTerm().trim().toLowerCase();
    const filter = this.activeFilter();
    const sort = this.sortBy();

    let list = this.courses().filter((c) => {
      const matchesTerm =
        !term ||
        c.title?.toLowerCase().includes(term) ||
        c.category?.toLowerCase().includes(term) ||
        c.description?.toLowerCase().includes(term);

      const status = this.statusOf(c.progressPercentage);
      const matchesFilter = filter === 'all' || filter === status;

      return matchesTerm && matchesFilter;
    });

    list = [...list].sort((a, b) => {
      if (sort === 'title') return (a.title ?? '').localeCompare(b.title ?? '');
      if (sort === 'progress-asc') return (a.progressPercentage ?? 0) - (b.progressPercentage ?? 0);
      return (b.progressPercentage ?? 0) - (a.progressPercentage ?? 0);
    });

    return list;
  });

  ngOnInit(): void {
    const stored = localStorage.getItem('username');
    if (stored) this.userName.set(stored);
    this.loadEnrolledCourses();
  }

  loadEnrolledCourses(): void {
    this.loading.set(true);
    this.courseService.getEnrolledCourses().subscribe({
      next: (enrollments) => {
        if (!enrollments || !enrollments.length) {
          this.courses.set([]);
          this.loading.set(false);
          return;
        }

        const detailsRequests = enrollments.map((e) =>
          this.courseService.getCourseDetails(e.courseId || e.course?.id)
        );

        let completed = 0;
        const loadedCourses: CourseDetailsDto[] = [];

        detailsRequests.forEach((req, idx) => {
          req.subscribe({
            next: (details) => {
              loadedCourses[idx] = details;
              completed++;
              if (completed === detailsRequests.length) {
                this.courses.set(loadedCourses.filter(Boolean));
                this.loading.set(false);
              }
            },
            error: () => {
              completed++;
              if (completed === detailsRequests.length) {
                this.courses.set(loadedCourses.filter(Boolean));
                this.loading.set(false);
              }
            },
          });
        });
      },
      error: () => {
        this.loading.set(false);
        this.errorMessage.set('Failed to load your enrolled courses.');
      },
    });
  }

  goToClassroom(courseId: number): void {
    this.router.navigate(['/student/classroom', courseId]);
  }

  statusOf(percentage?: number): CourseFilter {
    if (!percentage) return 'not-started';
    if (percentage === 100) return 'completed';
    return 'in-progress';
  }

  statusLabel(percentage?: number): string {
    const s = this.statusOf(percentage);
    if (s === 'completed') return 'Completed';
    if (s === 'in-progress') return 'In Progress';
    return 'Not Started';
  }
}
