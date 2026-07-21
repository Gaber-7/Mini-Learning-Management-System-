import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { CourseService } from '../../../Core/Services/course.service';
import { AuthService } from '../../../Core/Services/auth-service';

// واجهة مجردة لشكل الكورس
export interface Course {
  id: number;
  title: string;
  description: string;
  category: string;
  isPublished?: boolean;
  lessons?: any[];
  isEditing?: boolean; // خاصية اختارية للتحكم بالحالة في الصفحة
}

@Component({
  selector: 'app-course-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './course-management-component.html',
  styleUrls: ['./course-management-component.css'] // أو .scss حسب مشروعك
})
export class CourseManagementComponent implements OnInit {

  courses: Course[] = [];
  selectedCourse: Course | null = null;

  // Search
  searchText = '';

  // Messages
  successMessage = '';
  errorMessage = '';

  // Loading
  loading = false;

  // Form (Create / Edit Top Form)
  isEditing = false;
  courseForm = {
    id: 0,
    title: '',
    description: '',
    category: ''
  };

  // Add Lesson
  lessonForm = {
    title: '',
    content: ''
  };

  constructor(
    private courseService: CourseService,
    public authService: AuthService
  ) { }

  ngOnInit(): void {
    this.loadCourses();
  }

  // ===============================
  // Statistics
  // ===============================

  get totalCourses(): number {
    return this.courses.length;
  }

  get publishedCourses(): number {
    return this.courses.filter(x => x.isPublished).length;
  }

  get draftCourses(): number {
    return this.courses.filter(x => !x.isPublished).length;
  }

  // ===============================
  // Search
  // ===============================

  filteredCourses(): Course[] {
    if (!this.searchText.trim()) {
      return this.courses;
    }

    const query = this.searchText.toLowerCase();
    return this.courses.filter(course =>
      course.title?.toLowerCase().includes(query) ||
      course.category?.toLowerCase().includes(query)
    );
  }

  // ===============================
  // Alerts
  // ===============================

  showSuccess(message: string): void {
    this.successMessage = message;
    setTimeout(() => {
      this.successMessage = '';
    }, 3000);
  }

  showError(message: string): void {
    this.errorMessage = message;
    setTimeout(() => {
      this.errorMessage = '';
    }, 4000);
  }

  // ===============================
  // Load Courses
  // ===============================

  loadCourses(): void {
    this.loading = true;

    this.courseService.getAllCourses().subscribe({
      next: (data: Course[]) => {
        // إضافة خاصية isEditing بشكل افتراضي لكل كورس
        this.courses = data.map(c => ({ ...c, isEditing: false }));
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.showError('Failed to load courses');
      }
    });
  }

  // ===============================
  // Inline Card Toggle Edit (تم حل المشكلة هنا)
  // ===============================

  toggleEdit(course: Course): void {
    if (course.isEditing) {
      course.isEditing = false;
    } else {
      // إغلاق أي كورس آخر قيد التعديل
      this.courses.forEach(c => c.isEditing = false);
      course.isEditing = true;
    }
  }

  // ===============================
  // Create / Update Course Form
  // ===============================

  saveCourse(): void {
    if (!this.courseForm.title.trim()) {
      this.showError('Course title is required');
      return;
    }

    if (!this.courseForm.category.trim()) {
      this.showError('Course category is required');
      return;
    }

    if (this.isEditing) {
      this.courseService
        .updateCourse(this.courseForm.id, this.courseForm)
        .subscribe({
          next: () => {
            this.showSuccess('Course updated successfully');
            this.resetCourseForm();
            this.loadCourses();
          },
          error: () => {
            this.showError('Failed to update course');
          }
        });
    } else {
      this.courseService
        .createCourse(this.courseForm)
        .subscribe({
          next: () => {
            this.showSuccess('Course created successfully');
            this.resetCourseForm();
            this.loadCourses();
          },
          error: () => {
            this.showError('Failed to create course');
          }
        });
    }
  }

  editCourse(course: Course): void {
    this.isEditing = true;
    this.courseForm = {
      id: course.id,
      title: course.title,
      description: course.description,
      category: course.category
    };
  }

  resetCourseForm(): void {
    this.isEditing = false;
    this.courseForm = {
      id: 0,
      title: '',
      description: '',
      category: ''
    };
  }

  // ===============================
  // Delete Course
  // ===============================

  deleteCourse(courseId: number): void {
    if (!confirm('Delete this course ?')) {
      return;
    }

    this.courseService.deleteCourse(courseId).subscribe({
      next: () => {
        this.showSuccess('Course deleted successfully');
        this.loadCourses();
      },
      error: () => {
        this.showError('Failed to delete course');
      }
    });
  }

  // ===============================
  // Publish Course
  // ===============================

  publishCourse(courseId: number): void {
    this.courseService.publishCourse(courseId).subscribe({
      next: () => {
        this.showSuccess('Course published successfully');
        this.loadCourses();
      },
      error: () => {
        this.showError('Failed to publish course');
      }
    });
  }

  // ===============================
  // Lessons Management
  // ===============================

  manageLessons(course: Course): void {
    this.selectedCourse = course;
    this.lessonForm = {
      title: '',
      content: ''
    };
  }

  addLesson(): void {
    if (!this.selectedCourse) {
      return;
    }

    if (!this.lessonForm.title.trim()) {
      this.showError('Lesson title is required');
      return;
    }

    const lessonDto = {
      title: this.lessonForm.title,
      content: this.lessonForm.content,
      orderIndex: (this.selectedCourse.lessons?.length || 0) + 1
    };

    this.courseService
      .addLesson(this.selectedCourse.id, lessonDto)
      .subscribe({
        next: () => {
          this.showSuccess('Lesson added successfully');
          this.lessonForm = { title: '', content: '' };
          this.loadCourses();
        },
        error: () => {
          this.showError('Failed to add lesson');
        }
      });
  }

  moveUp(index: number): void {
    if (!this.selectedCourse || !this.selectedCourse.lessons || index === 0) return;

    const lessons = this.selectedCourse.lessons;
    [lessons[index], lessons[index - 1]] = [lessons[index - 1], lessons[index]];
    this.saveLessonOrder();
  }

  moveDown(index: number): void {
    if (!this.selectedCourse || !this.selectedCourse.lessons) return;

    const lessons = this.selectedCourse.lessons;
    if (index === lessons.length - 1) return;

    [lessons[index], lessons[index + 1]] = [lessons[index + 1], lessons[index]];
    this.saveLessonOrder();
  }

  saveLessonOrder(): void {
    if (!this.selectedCourse || !this.selectedCourse.lessons) return;

    const lessonIds = this.selectedCourse.lessons.map((x: any) => x.id);

    this.courseService
      .reorderLessons(this.selectedCourse.id, lessonIds)
      .subscribe({
        next: () => {
          this.showSuccess('Lesson order updated successfully');
        },
        error: () => {
          this.showError('Failed to update lesson order');
        }
      });
  }

  closeLessons(): void {
    this.selectedCourse = null;
  }
}