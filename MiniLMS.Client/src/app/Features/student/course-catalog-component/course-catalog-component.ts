import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CourseService } from '../../../Core/Services/course.service';
import { AuthService } from '../../../Core/Services/auth-service';
import { CourseCheckoutModalComponent } from '../../../Shared/course-checkout-modal/course-checkout-modal.component';
import { NotificationBellComponent } from '../../../Shared/notification-bell/notification-bell.component';
import { GamificationWidgetComponent } from '../../../Shared/gamification-widget/gamification-widget.component';

@Component({
  selector: 'app-course-catalog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    CourseCheckoutModalComponent,
    NotificationBellComponent,
    GamificationWidgetComponent
  ],
  templateUrl: './course-catalog-component.html',
  styleUrls: ['./course-catalog-component.css']
})
export class CourseCatalogComponent implements OnInit {
  courses: any[] = [];
  filteredCourses: any[] = [];
  categories: string[] = [];
  
  searchTerm: string = '';
  selectedCategory: string = '';

  loading: boolean = false;
  enrollingCourseId: number | null = null;
  selectedCourseForCheckout: any | null = null;
  errorMessage: string = '';
  successMessage: string = '';

  constructor(
    private courseService: CourseService,
    public authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadCourses();
  }

  loadCourses(): void {
    this.loading = true;
    this.errorMessage = '';

    this.courseService.getPublishedCourses().subscribe({
      next: (data) => {
        this.courses = data;
        this.filteredCourses = data;
        this.categories = [...new Set(data.map((c: any) => c.category))];
        this.loading = false;
      },
      error: (err) => {
        this.errorMessage = 'Failed to load available courses. Please try again later.';
        this.loading = false;
      }
    });
  }

  applyFilter(): void {
    this.filteredCourses = this.courses.filter(course => {
      const matchesSearch = 
        course.title?.toLowerCase().includes(this.searchTerm.toLowerCase()) || 
        course.description?.toLowerCase().includes(this.searchTerm.toLowerCase());

      const matchesCategory = 
        this.selectedCategory === '' || course.category === this.selectedCategory;

      return matchesSearch && matchesCategory;
    });
  }

  getLessonsCount(course: any): number {
    if (course.sections && course.sections.length > 0) {
      return course.sections.reduce((sum: number, s: any) => sum + (s.lessons?.length || 0), 0);
    }
    return course.lessons?.length || 0;
  }

  enroll(course: any): void {
    if (course.price && course.price > 0) {
      this.selectedCourseForCheckout = course;
      return;
    }

    const courseId = course.id;
    this.enrollingCourseId = courseId;
    this.errorMessage = '';
    this.successMessage = '';

    this.courseService.enrollInCourse(courseId).subscribe({
      next: () => {
        this.enrollingCourseId = null;
        this.successMessage = 'Successfully enrolled! Redirecting to your dashboard...';
        
        setTimeout(() => {
          this.router.navigate(['/student/dashboard']);
        }, 1200);
      },
      error: (err) => {
        this.enrollingCourseId = null;
        this.errorMessage = err.error?.message || 'Enrollment failed. You might already be enrolled in this course.';
      }
    });
  }

  onEnrolledViaPayment(res: any): void {
    this.selectedCourseForCheckout = null;
    this.successMessage = 'تم الدفع والاشتراك بنجاح! جاري توجيهك إلى لوحة التحكم...';
    setTimeout(() => {
      this.router.navigate(['/student/dashboard']);
    }, 1500);
  }
}