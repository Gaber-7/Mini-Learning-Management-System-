import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { CourseService } from '../../../Core/Services/course.service';

@Component({
  selector: 'app-student-dashboard',
  standalone: true,
  imports: [CommonModule],
templateUrl: 'dashboard-component.html',
//./dashboard-component.component.html
})
export class DashboardComponent implements OnInit {
  enrolledCourses: any[] = [];
  selectedCourse: any = null; // لمتابعة الدروس عند الضغط على كورس

  constructor(private courseService: CourseService) {}

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.courseService.getEnrolledCourses().subscribe({
      next: (data) => {
        this.enrolledCourses = data;
      }
    });
  }

  // تحديد كورس لعرض دروسه بالأسفل ومتابعتها
  viewCourseLessons(course: any): void {
    this.selectedCourse = course;
  }

  toggleLesson(lesson: any): void {
    const nextState = !lesson.isCompleted;
    this.courseService.toggleLessonCompletion(this.selectedCourse.id, lesson.id, nextState).subscribe({
      next: () => {
        lesson.isCompleted = nextState;
        this.loadDashboard(); // إعادة تحميل لحساب شريط التقدم والـ Badge الكلي للكورس مجدداً
      }
    });
  }
}
