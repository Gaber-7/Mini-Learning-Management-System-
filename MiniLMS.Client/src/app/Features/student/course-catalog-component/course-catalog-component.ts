import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CourseService } from '../../../Core/Services/course.service';
import { AuthService } from '../../../Core/Services/auth-service';

@Component({
  selector: 'app-course-catalog',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: 'course-catalog-component.html',
})
export class CourseCatalogComponent implements OnInit {
  courses: any[] = [];
  filteredCourses: any[] = [];
  searchTerm: string = '';
  selectedCategory: string = '';
  categories: string[] = [];

  constructor(private courseService: CourseService,
  public authService: AuthService
  ) {}

  ngOnInit(): void {
    this.loadCourses();
  }

  loadCourses(): void {
    this.courseService.getPublishedCourses().subscribe({
      next: (data) => {
        this.courses = data;
        this.filteredCourses = data;
        // استخراج التصنيفات المتاحة بدون تكرار للفلترة
        this.categories = [...new Set(data.map(c => c.category))];
      }
    });
  }

  applyFilter(): void {
    this.filteredCourses = this.courses.filter(course => {
      const matchesSearch = course.title.toLowerCase().includes(this.searchTerm.toLowerCase()) || 
                            course.description.toLowerCase().includes(this.searchTerm.toLowerCase());
      const matchesCategory = this.selectedCategory === '' || course.category === this.selectedCategory;
      return matchesSearch && matchesCategory;
    });
  }

enroll(courseId: number): void {
  this.courseService.enrollInCourse(courseId).subscribe({
    next: () => {
      alert('You have successfully enrolled in the course. Go to your dashboard to start learning.');
      this.loadCourses();
    }
  });
}
}
