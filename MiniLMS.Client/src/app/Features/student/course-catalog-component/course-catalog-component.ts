import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CourseService } from '../../../Core/Services/course.service';
import { AuthService } from '../../../Core/Services/auth-service';

@Component({
  selector: 'app-course-catalog',
  standalone: true,
  imports: [CommonModule, FormsModule],
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

  enroll(courseId: number): void {
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
}