import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  Course,
  CreateLessonDto,
  CreateSectionDto,
  InstructorProfileDto,
  InstructorStudentDto,
  Lesson,
  Section,
  UpdateInstructorProfileDto
} from '../../Models/Course';

@Injectable({
  providedIn: 'root'
})
export class InstructorService {

  private instructorApiUrl = 'https://localhost:7070/api/InstructorCourses';
  private profileApiUrl = 'https://localhost:7070/api/Instructors';

  constructor(private http: HttpClient) { }

  // ================= Profile =================

  getProfile(): Observable<InstructorProfileDto> {
    return this.http.get<InstructorProfileDto>(`${this.profileApiUrl}/profile`);
  }

  updateProfile(data: UpdateInstructorProfileDto): Observable<any> {
    return this.http.put(`${this.profileApiUrl}/profile`, data);
  }

  getPublicProfile(id: number): Observable<InstructorProfileDto> {
    return this.http.get<InstructorProfileDto>(`${this.profileApiUrl}/${id}/public`);
  }

  // ================= Courses =================

  getMyCourses(): Observable<Course[]> {
    return this.http.get<Course[]>(`${this.instructorApiUrl}`);
  }

  getCourseById(courseId: number): Observable<Course> {
    return this.http.get<Course>(`${this.instructorApiUrl}/${courseId}`);
  }

  createCourse(courseData: { title: string; description: string; category: string }): Observable<Course> {
    return this.http.post<Course>(`${this.instructorApiUrl}`, courseData);
  }

  updateCourse(courseId: number, courseData: { title: string; description: string; category: string }): Observable<any> {
    return this.http.put(`${this.instructorApiUrl}/${courseId}`, courseData);
  }

  deleteCourse(courseId: number): Observable<any> {
    return this.http.delete(`${this.instructorApiUrl}/${courseId}`);
  }

  submitForReview(courseId: number): Observable<any> {
    return this.http.post(`${this.instructorApiUrl}/${courseId}/submit-review`, {});
  }

  // ================= Students =================

  getMyStudents(): Observable<InstructorStudentDto[]> {
    return this.http.get<InstructorStudentDto[]>(`${this.instructorApiUrl}/students`);
  }

  // ================= Sections =================

  addSection(courseId: number, data: CreateSectionDto): Observable<Section> {
    return this.http.post<Section>(`${this.instructorApiUrl}/${courseId}/sections`, data);
  }

  updateSection(sectionId: number, data: CreateSectionDto): Observable<any> {
    return this.http.put(`${this.instructorApiUrl}/sections/${sectionId}`, data);
  }

  deleteSection(sectionId: number): Observable<any> {
    return this.http.delete(`${this.instructorApiUrl}/sections/${sectionId}`);
  }

  // ================= Lessons =================

  addLessonToSection(sectionId: number, data: CreateLessonDto): Observable<Lesson> {
    return this.http.post<Lesson>(`${this.instructorApiUrl}/sections/${sectionId}/lessons`, data);
  }

  updateLesson(lessonId: number, data: CreateLessonDto): Observable<any> {
    return this.http.put(`${this.instructorApiUrl}/lessons/${lessonId}`, data);
  }

  deleteLesson(lessonId: number): Observable<any> {
    return this.http.delete(`${this.instructorApiUrl}/lessons/${lessonId}`);
  }
}
