import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  AiExplainRequestDto,
  AiPracticeQuestionDto,
  AiPracticeQuestionsRequestDto,
  AiResponseDto,
  AiSummarizeRequestDto
} from '../../Models/GenAlpha';

@Injectable({
  providedIn: 'root'
})
export class AiTutorService {
  private apiUrl = 'https://localhost:7070/api/AiTutor';

  constructor(private http: HttpClient) {}

  explainConcept(request: AiExplainRequestDto): Observable<AiResponseDto> {
    return this.http.post<AiResponseDto>(`${this.apiUrl}/explain`, request);
  }

  summarizeLesson(request: AiSummarizeRequestDto): Observable<AiResponseDto> {
    return this.http.post<AiResponseDto>(`${this.apiUrl}/summarize`, request);
  }

  generatePracticeQuestions(request: AiPracticeQuestionsRequestDto): Observable<AiPracticeQuestionDto[]> {
    return this.http.post<AiPracticeQuestionDto[]>(`${this.apiUrl}/practice-questions`, request);
  }
}
