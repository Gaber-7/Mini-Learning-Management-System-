import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CourseService } from '../../../Core/Services/course.service';
import { AuthService } from '../../../Core/Services/auth-service';
import { Router } from '@angular/router';



@Component({
  selector: 'app-course-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: 'course-management-component.html',
})
export class CourseManagementComponent implements OnInit {
  courses: any[] = [];
  selectedCourse: any = null;
  viewingStudents: any = null;
  studentsProgress: any[] = [];

  // نموذج الكورس (Create/Edit)
  courseForm = { id: 0, title: '', description: '', category: '' };
  isEditing = false;

  // نموذج الدرس الجديد
  newLessonTitle = '';

  constructor(
    private courseService: CourseService,
     public authService: AuthService,
    private router: Router // أضف هذا

  ) {}

  ngOnInit(): void {
    this.loadCourses();
  }

  loadCourses(): void {
    this.courseService.getAllCourses().subscribe({
      next: (data) => this.courses = data
    });
  }

  saveCourse(): void {
    if (!this.courseForm.title || !this.courseForm.category) return;

    if (this.isEditing) {
      this.courseService.updateCourse(this.courseForm.id, this.courseForm).subscribe({
        next: () => {
          alert('Course updated successfully!');
          this.resetCourseForm();
          this.loadCourses();
        }
      });
    } else {
      this.courseService.createCourse(this.courseForm).subscribe({
        next: () => {
          alert('Course created successfully!');
          this.resetCourseForm();
          this.loadCourses();
        }
      });
    }
  }

  editCourse(course: any): void {
    this.isEditing = true;
    this.courseForm = { ...course };
    this.selectedCourse = course;
  }

  publishCourse(courseId: number): void {
    this.courseService.publishCourse(courseId).subscribe({
      next: () => {
        alert('Course published successfully and is now available in the catalog!');
        this.loadCourses();
      }
    });
  }

  addLesson(): void {
    if (!this.newLessonTitle || !this.selectedCourse) return;
    this.courseService.addLesson(this.selectedCourse.id, { title: this.newLessonTitle }).subscribe({
      next: () => {
        alert('Lesson added successfully!');
        this.newLessonTitle = '';
        this.loadCourses();
        // تحديث القائمة المعروضة حالياً
        const updated = this.courses.find(c => c.id === this.selectedCourse.id);
        if (updated) this.selectedCourse = updated;
      }
    });
  }

  deleteLesson(lessonId: number): void {
    if (!confirm('Are you sure you want to delete this lesson?')) return;
    this.courseService.deleteLesson(this.selectedCourse.id, lessonId).subscribe({
      next: () => {
        alert('Lesson deleted successfully.');
        this.loadCourses();
        const updated = this.courses.find(c => c.id === this.selectedCourse.id);
        if (updated) this.selectedCourse = updated;
      }
    });
  }

  viewStudents(course: any): void {
    this.viewingStudents = course;
    this.courseService.getCourseStudentsProgress(course.id).subscribe({
      next: (data) => this.studentsProgress = data
    });
  }

  resetCourseForm(): void {
    this.courseForm = { id: 0, title: '', description: '', category: '' };
    this.isEditing = false;
    this.selectedCourse = null;
  }
}
