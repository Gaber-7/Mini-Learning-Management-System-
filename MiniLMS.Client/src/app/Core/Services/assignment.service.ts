import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  AssignmentDto,
  AssignmentSubmissionDto,
  CreateAssignmentDto,
  GradeAssignmentDto,
  SubmitAssignmentDto
} from '../../Models/Course';

@Injectable({
  providedIn: 'root'
})
export class AssignmentService {
  private apiUrl = 'https://localhost:7070/api/Assignments';

  constructor(private http: HttpClient) {}

  getCourseAssignments(courseId: number): Observable<AssignmentDto[]> {
    return this.http.get<AssignmentDto[]>(`${this.apiUrl}/course/${courseId}`);
  }

  getAssignmentById(assignmentId: number): Observable<AssignmentDto> {
    return this.http.get<AssignmentDto>(`${this.apiUrl}/${assignmentId}`);
  }

  createAssignment(courseId: number, data: CreateAssignmentDto): Observable<AssignmentDto> {
    return this.http.post<AssignmentDto>(`${this.apiUrl}/course/${courseId}`, data);
  }

  updateAssignment(assignmentId: number, data: CreateAssignmentDto): Observable<any> {
    return this.http.put(`${this.apiUrl}/${assignmentId}`, data);
  }

  deleteAssignment(assignmentId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${assignmentId}`);
  }

  submitAssignment(assignmentId: number, data: SubmitAssignmentDto): Observable<AssignmentSubmissionDto> {
    return this.http.post<AssignmentSubmissionDto>(`${this.apiUrl}/${assignmentId}/submit`, data);
  }

  getSubmissions(assignmentId: number): Observable<AssignmentSubmissionDto[]> {
    return this.http.get<AssignmentSubmissionDto[]>(`${this.apiUrl}/${assignmentId}/submissions`);
  }

  gradeSubmission(submissionId: number, data: GradeAssignmentDto): Observable<any> {
    return this.http.post(`${this.apiUrl}/submissions/${submissionId}/grade`, data);
  }
}
