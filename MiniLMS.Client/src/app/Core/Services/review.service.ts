import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CourseRatingSummaryDto, CourseReviewDto, CreateCourseReviewDto } from '../../Models/Course';

@Injectable({
  providedIn: 'root'
})
export class ReviewService {
  private apiUrl = 'https://localhost:7070/api/Reviews';

  constructor(private http: HttpClient) {}

  getCourseReviews(courseId: number): Observable<CourseReviewDto[]> {
    return this.http.get<CourseReviewDto[]>(`${this.apiUrl}/course/${courseId}`);
  }

  getRatingSummary(courseId: number): Observable<CourseRatingSummaryDto> {
    return this.http.get<CourseRatingSummaryDto>(`${this.apiUrl}/course/${courseId}/summary`);
  }

  addOrUpdateReview(courseId: number, data: CreateCourseReviewDto): Observable<CourseReviewDto> {
    return this.http.post<CourseReviewDto>(`${this.apiUrl}/course/${courseId}`, data);
  }

  deleteReview(reviewId: number): Observable<any> {
    return this.http.delete(`${this.apiUrl}/${reviewId}`);
  }
}
