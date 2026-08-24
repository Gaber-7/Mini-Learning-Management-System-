import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  CreateLessonQuestionDto,
  CreateLessonReplyDto,
  LessonQuestionDto,
  LessonReplyDto
} from '../../Models/Course';

@Injectable({
  providedIn: 'root'
})
export class QnAService {
  private apiUrl = 'https://localhost:7070/api/QnA';

  constructor(private http: HttpClient) {}

  getLessonQuestions(lessonId: number): Observable<LessonQuestionDto[]> {
    return this.http.get<LessonQuestionDto[]>(`${this.apiUrl}/lessons/${lessonId}`);
  }

  getQuestionById(questionId: number): Observable<LessonQuestionDto> {
    return this.http.get<LessonQuestionDto>(`${this.apiUrl}/questions/${questionId}`);
  }

  askQuestion(lessonId: number, data: CreateLessonQuestionDto): Observable<LessonQuestionDto> {
    return this.http.post<LessonQuestionDto>(`${this.apiUrl}/lessons/${lessonId}`, data);
  }

  addReply(questionId: number, data: CreateLessonReplyDto): Observable<LessonReplyDto> {
    return this.http.post<LessonReplyDto>(`${this.apiUrl}/questions/${questionId}/replies`, data);
  }

  toggleResolved(questionId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/questions/${questionId}/toggle-resolved`, {});
  }

  markAcceptedAnswer(replyId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/replies/${replyId}/accept-answer`, {});
  }

  upvoteQuestion(questionId: number): Observable<{ upvotesCount: number }> {
    return this.http.post<{ upvotesCount: number }>(`${this.apiUrl}/questions/${questionId}/upvote`, {});
  }

  upvoteReply(replyId: number): Observable<{ upvotesCount: number }> {
    return this.http.post<{ upvotesCount: number }>(`${this.apiUrl}/replies/${replyId}/upvote`, {});
  }
}
