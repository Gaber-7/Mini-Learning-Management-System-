import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CreateQuizDto, QuizDto, QuizResultDto, SubmitQuizDto } from '../../Models/Course';

@Injectable({
  providedIn: 'root'
})
export class QuizService {
  private apiUrl = 'https://localhost:7070/api/Quizzes';

  constructor(private http: HttpClient) {}

  getCourseQuizzes(courseId: number): Observable<QuizDto[]> {
    return this.http.get<QuizDto[]>(`${this.apiUrl}/course/${courseId}`);
  }

  getQuizById(quizId: number): Observable<QuizDto> {
    return this.http.get<QuizDto>(`${this.apiUrl}/${quizId}`);
  }

  createQuiz(courseId: number, data: CreateQuizDto): Observable<QuizDto> {
    return this.http.post<QuizDto>(`${this.apiUrl}/course/${courseId}`, data);
  }

  updateQuiz(quizId: number, data: CreateQuizDto): Observable<any> {
    return this.http.put(`${this.apiUrl}/${quizId}`, data);
  }

  deleteQuiz(quizId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${quizId}`);
  }

  submitQuiz(quizId: number, answers: SubmitQuizDto): Observable<QuizResultDto> {
    return this.http.post<QuizResultDto>(`${this.apiUrl}/${quizId}/submit`, answers);
  }

  getMyAttempts(quizId: number): Observable<QuizResultDto[]> {
    return this.http.get<QuizResultDto[]>(`${this.apiUrl}/${quizId}/attempts`);
  }
}
