import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BadgeDto, LeaderboardItemDto, StudentGamificationDto } from '../../Models/GenAlpha';
import { AuthService } from './auth-service';

@Injectable({
  providedIn: 'root'
})
export class GamificationService {
  private apiUrl = 'https://localhost:7070/api/Gamification';

  constructor(private http: HttpClient, private authService: AuthService) {}

  private getAuthHeaders(): HttpHeaders {
    const token = this.authService.getToken();
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
  }

  getStudentProfile(studentId: number): Observable<StudentGamificationDto> {
    return this.http.get<StudentGamificationDto>(`${this.apiUrl}/profile/${studentId}`);
  }

  getMyProfile(): Observable<StudentGamificationDto> {
    return this.http.get<StudentGamificationDto>(`${this.apiUrl}/my-profile`, { headers: this.getAuthHeaders() });
  }

  getBadges(studentId: number): Observable<BadgeDto[]> {
    return this.http.get<BadgeDto[]>(`${this.apiUrl}/badges/${studentId}`);
  }

  getLeaderboard(top: number = 10): Observable<LeaderboardItemDto[]> {
    return this.http.get<LeaderboardItemDto[]>(`${this.apiUrl}/leaderboard?top=${top}`);
  }

  awardXP(amount: number, reason: string, studentId?: number): Observable<StudentGamificationDto> {
    const body = { studentId: studentId || 0, amount, reason };
    return this.http.post<StudentGamificationDto>(`${this.apiUrl}/award-xp`, body, { headers: this.getAuthHeaders() });
  }
}
