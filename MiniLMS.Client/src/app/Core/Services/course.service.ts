import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CourseService {

  private adminApiUrl =
    'https://localhost:7070/api/AdminCourses';

  private studentApiUrl =
    'https://localhost:7070/api/StudentCourses';

  constructor(private http: HttpClient) { }

  // ==========================================
  // STUDENT AREA
  // ==========================================

  // Available Courses
  getPublishedCourses(
    search?: string,
    category?: string
  ): Observable<any[]> {

    let url =
      `${this.studentApiUrl}/available`;

    const params: string[] = [];

    if (search) {
      params.push(`search=${search}`);
    }

    if (category) {
      params.push(`category=${category}`);
    }

    if (params.length > 0) {
      url += `?${params.join('&')}`;
    }

    return this.http.get<any[]>(url);
  }

  // Enroll Course
  enrollInCourse(
    courseId: number
  ): Observable<any> {

    return this.http.post(
      `${this.studentApiUrl}/enroll/${courseId}`,
      {}
    );
  }

  // My Courses
  getEnrolledCourses(): Observable<any[]> {

    return this.http.get<any[]>(
      `${this.studentApiUrl}/my-courses`
    );
  }

  // Course Details
  getCourseDetails(
    courseId: number
  ): Observable<any> {

    return this.http.get<any>(
      `${this.studentApiUrl}/details/${courseId}`
    );
  }

  // Complete Lesson
  toggleLessonCompletion(
enrollmentId: number, lessonId: number, nextState: boolean  ): Observable<any> {

    return this.http.post(
      `${this.studentApiUrl}/enrollments/${enrollmentId}/complete-lesson/${lessonId}`,
      {}
    );
  }

  // ==========================================
  // ADMIN AREA
  // ==========================================

  // Get All Courses
  getAllCourses(): Observable<any[]> {

    return this.http.get<any[]>(
      `${this.adminApiUrl}`
    );
  }

  // Create Course
  createCourse(
    courseData: any
  ): Observable<any> {

    return this.http.post(
      `${this.adminApiUrl}`,
      courseData
    );
  }

  // Update Course
  updateCourse(
    courseId: number,
    courseData: any
  ): Observable<any> {

    return this.http.put(
      `${this.adminApiUrl}/${courseId}`,
      courseData
    );
  }

  // Delete Course
  deleteCourse(
    courseId: number
  ): Observable<any> {

    return this.http.delete(
      `${this.adminApiUrl}/${courseId}`
    );
  }

  // Publish Course
  publishCourse(
    courseId: number
  ): Observable<any> {

    return this.http.post(
      `${this.adminApiUrl}/${courseId}/publish`,
      {}
    );
  }

  // Add Lesson
  addLesson(
    courseId: number,
    lessonData: any
  ): Observable<any> {

    return this.http.post(
      `${this.adminApiUrl}/${courseId}/lessons`,
      lessonData
    );
  }

  // Reorder Lessons
  reorderLessons(
    courseId: number,
    lessonIds: number[]
  ): Observable<any> {

    return this.http.post(
      `${this.adminApiUrl}/${courseId}/lessons/reorder`,
      lessonIds
    );
  }

}