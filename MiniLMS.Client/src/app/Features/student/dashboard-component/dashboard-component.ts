import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CourseService } from '../../../Core/Services/course.service';

@Component({
  selector: 'app-student-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: 'dashboard-component.html',
})
export class DashboardComponent implements OnInit {
  enrolledCourses: any[] = [];
  selectedCourse: any = null; 
  loading = false;

  successMessage = '';

  errorMessage = '';
  constructor(private courseService: CourseService) { }

  ngOnInit(): void {
    this.loadDashboard();
  }

 loadDashboard(): void {

  this.loading = true;

  this.courseService.getEnrolledCourses().subscribe({

    next: (data) => {

      this.enrolledCourses = data;

      this.loading = false;
    },

    error: () => {

      this.loading = false;

      this.errorMessage =
        'Failed to load enrolled courses';
    }

  });

}

 viewCourseLessons(course: any): void {

  this.courseService
    .getCourseDetails(course.courseId)
    .subscribe({

      next: (data) => {

        data.enrollmentId = course.id;

        this.selectedCourse = data;
      }

    });

}

 toggleLesson(lesson: any): void {

  const nextState = !lesson.isCompleted;

  this.courseService
    .toggleLessonCompletion(
      this.selectedCourse.id,
      lesson.id,
      nextState
    )
    .subscribe({

      next: () => {

        lesson.isCompleted = nextState;

        this.successMessage =
          'Lesson updated successfully';

        this.loadDashboard();
      },

      error: () => {

        this.errorMessage =
          'Failed to update lesson';
      }

    });

}
}
