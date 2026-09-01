import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface StudentListItem {
  id: number;
  username: string;
  fullName: string;
  email: string;
  totalEnrollments: number;
  completedCourses: number;
  inProgressCourses: number;
  enrollments?: any[];
}

export interface CreateStudentDto {
  username: string;
  fullName: string;
  email: string;
  password?: string;
}

export interface UpdateStudentDto {
  fullName: string;
  email: string;
  password?: string;
}

export interface InstructorListItem {
  id: number;
  username: string;
  fullName: string;
  email: string;
  headline?: string;
  bio?: string;
  profilePictureUrl?: string;
  websiteUrl?: string;
  linkedInUrl?: string;
  gitHubUrl?: string;
  youTubeUrl?: string;
  totalCourses: number;
  publishedCoursesCount: number;
  totalStudentsCount: number;
}

export interface CreateInstructorDto {
  username: string;
  fullName: string;
  email: string;
  password?: string;
  headline?: string;
  bio?: string;
  profilePictureUrl?: string;
  websiteUrl?: string;
  linkedInUrl?: string;
  gitHubUrl?: string;
  youTubeUrl?: string;
}

export interface UpdateInstructorDto {
  fullName: string;
  email: string;
  password?: string;
  headline?: string;
  bio?: string;
  profilePictureUrl?: string;
  websiteUrl?: string;
  linkedInUrl?: string;
  gitHubUrl?: string;
  youTubeUrl?: string;
}

export interface AdminReviewItem {
  id: number;
  courseId: number;
  courseTitle: string;
  studentId: number;
  studentName: string;
  rating: number;
  comment: string;
  createdAt: string;
  isApproved: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class AdminUsersService {
  private apiUrl = 'https://localhost:7070/api/AdminUsers';

  constructor(private http: HttpClient) {}

  // ==================== STUDENTS ====================

  getStudents(): Observable<StudentListItem[]> {
    return this.http.get<StudentListItem[]>(`${this.apiUrl}/students`);
  }

  getStudentById(id: number): Observable<StudentListItem> {
    return this.http.get<StudentListItem>(`${this.apiUrl}/students/${id}`);
  }

  createStudent(dto: CreateStudentDto): Observable<StudentListItem> {
    return this.http.post<StudentListItem>(`${this.apiUrl}/students`, dto);
  }

  updateStudent(id: number, dto: UpdateStudentDto): Observable<any> {
    return this.http.put(`${this.apiUrl}/students/${id}`, dto);
  }

  deleteStudent(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/students/${id}`);
  }

  // ==================== INSTRUCTORS ====================

  getInstructors(): Observable<InstructorListItem[]> {
    return this.http.get<InstructorListItem[]>(`${this.apiUrl}/instructors`);
  }

  getInstructorById(id: number): Observable<InstructorListItem> {
    return this.http.get<InstructorListItem>(`${this.apiUrl}/instructors/${id}`);
  }

  createInstructor(dto: CreateInstructorDto): Observable<InstructorListItem> {
    return this.http.post<InstructorListItem>(`${this.apiUrl}/instructors`, dto);
  }

  updateInstructor(id: number, dto: UpdateInstructorDto): Observable<any> {
    return this.http.put(`${this.apiUrl}/instructors/${id}`, dto);
  }

  deleteInstructor(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/instructors/${id}`);
  }

  // ==================== REVIEWS ====================

  getReviews(): Observable<AdminReviewItem[]> {
    return this.http.get<AdminReviewItem[]>(`${this.apiUrl}/reviews`);
  }

  toggleReviewApproval(id: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/reviews/${id}/toggle-approval`, {});
  }

  deleteReview(id: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/reviews/${id}`);
  }
}
