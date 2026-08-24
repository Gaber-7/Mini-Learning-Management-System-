import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Course, CourseDetailsDto, CreateLessonDto, CreateSectionDto, Lesson, Section } from '../../Models/Course';

@Injectable({
  providedIn: 'root'
})
export class CourseService {

  private adminApiUrl = 'https://localhost:7070/api/AdminCourses';
  private studentApiUrl = 'https://localhost:7070/api/StudentCourses';

  constructor(private http: HttpClient) { }

  // ==========================================
  // STUDENT AREA
  // ==========================================

  getPublishedCourses(search?: string, category?: string): Observable<Course[]> {
    let url = `${this.studentApiUrl}/available`;
    const params: string[] = [];

    if (search) {
      params.push(`search=${encodeURIComponent(search)}`);
    }

    if (category) {
      params.push(`category=${encodeURIComponent(category)}`);
    }

    if (params.length > 0) {
      url += `?${params.join('&')}`;
    }

    return this.http.get<Course[]>(url);
  }

  enrollInCourse(courseId: number): Observable<any> {
    return this.http.post(`${this.studentApiUrl}/enroll/${courseId}`, {});
  }

  getEnrolledCourses(): Observable<any[]> {
    return this.http.get<any[]>(`${this.studentApiUrl}/my-courses`);
  }

  getCourseDetails(courseId: number): Observable<CourseDetailsDto> {
    return this.http.get<CourseDetailsDto>(`${this.studentApiUrl}/details/${courseId}`);
  }

  toggleLessonCompletion(enrollmentId: number, lessonId: number, nextState?: boolean): Observable<any> {
    return this.http.post(
      `${this.studentApiUrl}/enrollments/${enrollmentId}/complete-lesson/${lessonId}`,
      {}
    );
  }

  updateWatchProgress(enrollmentId: number, lessonId: number, lastWatchedSeconds: number, watchPercentage: number, forceCompleted: boolean = false): Observable<any> {
    return this.http.post(
      `${this.studentApiUrl}/enrollments/${enrollmentId}/lessons/${lessonId}/watch-progress`,
      { lastWatchedSeconds, watchPercentage, forceCompleted }
    );
  }

  // ==========================================
  // ADMIN AREA
  // ==========================================

  getAllCourses(): Observable<Course[]> {
    return this.http.get<Course[]>(`${this.adminApiUrl}`);
  }

  getCourseById(courseId: number): Observable<Course> {
    return this.http.get<Course>(`${this.adminApiUrl}/${courseId}`);
  }

  createCourse(courseData: { title: string; description: string; category: string }): Observable<Course> {
    return this.http.post<Course>(`${this.adminApiUrl}`, courseData);
  }

  updateCourse(courseId: number, courseData: { title: string; description: string; category: string }): Observable<any> {
    return this.http.put(`${this.adminApiUrl}/${courseId}`, courseData);
  }

  deleteCourse(courseId: number): Observable<any> {
    return this.http.delete(`${this.adminApiUrl}/${courseId}`);
  }

  publishCourse(courseId: number): Observable<any> {
    return this.http.post(`${this.adminApiUrl}/${courseId}/publish`, {});
  }

  // Course Approval Workflow
  getPendingReviewCourses(): Observable<Course[]> {
    return this.http.get<Course[]>(`${this.adminApiUrl}/pending-review`);
  }

  approveCourse(courseId: number): Observable<any> {
    return this.http.post(`${this.adminApiUrl}/${courseId}/approve`, {});
  }

  rejectCourse(courseId: number, reason: string): Observable<any> {
    return this.http.post(`${this.adminApiUrl}/${courseId}/reject`, { reason });
  }

  // --- SECTIONS ---

  addSection(courseId: number, sectionData: CreateSectionDto): Observable<Section> {
    return this.http.post<Section>(`${this.adminApiUrl}/${courseId}/sections`, sectionData);
  }

  updateSection(sectionId: number, sectionData: CreateSectionDto): Observable<any> {
    return this.http.put(`${this.adminApiUrl}/sections/${sectionId}`, sectionData);
  }

  deleteSection(sectionId: number): Observable<any> {
    return this.http.delete(`${this.adminApiUrl}/sections/${sectionId}`);
  }

  reorderSections(courseId: number, sectionIds: number[]): Observable<any> {
    return this.http.post(`${this.adminApiUrl}/${courseId}/sections/reorder`, sectionIds);
  }

  // --- LESSONS ---

  addLessonToSection(sectionId: number, lessonData: CreateLessonDto): Observable<Lesson> {
    return this.http.post<Lesson>(`${this.adminApiUrl}/sections/${sectionId}/lessons`, lessonData);
  }

  addLesson(courseId: number, lessonData: CreateLessonDto): Observable<Lesson> {
    return this.http.post<Lesson>(`${this.adminApiUrl}/${courseId}/lessons`, lessonData);
  }

  updateLesson(lessonId: number, lessonData: CreateLessonDto): Observable<any> {
    return this.http.put(`${this.adminApiUrl}/lessons/${lessonId}`, lessonData);
  }

  deleteLesson(lessonId: number): Observable<any> {
    return this.http.delete(`${this.adminApiUrl}/lessons/${lessonId}`);
  }

  reorderLessonsInSection(sectionId: number, lessonIds: number[]): Observable<any> {
    return this.http.post(`${this.adminApiUrl}/sections/${sectionId}/lessons/reorder`, lessonIds);
  }

  reorderLessons(courseId: number, lessonIds: number[]): Observable<any> {
    return this.http.post(`${this.adminApiUrl}/${courseId}/lessons/reorder`, lessonIds);
  }
}