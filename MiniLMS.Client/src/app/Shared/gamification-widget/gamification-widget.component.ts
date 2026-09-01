import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GamificationService } from '../../Core/Services/gamification.service';
import { BadgeDto, LeaderboardItemDto, StudentGamificationDto } from '../../Models/GenAlpha';
import { AuthService } from '../../Core/Services/auth-service';

@Component({
  selector: 'app-gamification-widget',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './gamification-widget.component.html',
  styleUrl: './gamification-widget.component.css'
})
export class GamificationWidgetComponent implements OnInit {
  readonly profile = signal<StudentGamificationDto | null>(null);
  readonly leaderboard = signal<LeaderboardItemDto[]>([]);
  readonly allBadges = signal<BadgeDto[]>([]);
  readonly showLeaderboard = signal(false);
  readonly showBadgesModal = signal(false);

  constructor(
    private gamificationService: GamificationService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    if (this.authService.isLoggedIn()) {
      this.loadProfile();
    }
  }

  loadProfile(): void {
    this.gamificationService.getMyProfile().subscribe({
      next: (data) => this.profile.set(data),
      error: (err) => console.warn('Could not load gamification profile', err)
    });
  }

  openLeaderboard(): void {
    this.showLeaderboard.set(true);
    this.gamificationService.getLeaderboard(10).subscribe({
      next: (data) => this.leaderboard.set(data),
      error: (err) => console.warn('Could not load leaderboard', err)
    });
  }

  closeLeaderboard(): void {
    this.showLeaderboard.set(false);
  }

  openBadgesModal(): void {
    this.showBadgesModal.set(true);
    const studentId = this.profile()?.studentId || 0;
    if (studentId > 0) {
      this.gamificationService.getBadges(studentId).subscribe({
        next: (badges) => this.allBadges.set(badges),
        error: (err) => console.warn('Could not load badges', err)
      });
    }
  }

  closeBadgesModal(): void {
    this.showBadgesModal.set(false);
  }
}
