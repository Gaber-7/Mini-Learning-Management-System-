import { Component, OnInit, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { NotificationService } from '../../Core/Services/notification.service';
import { NotificationDto } from '../../Models/GenAlpha';

@Component({
  selector: 'app-notification-bell',
  standalone: true,
  imports: [CommonModule, DatePipe],
  templateUrl: './notification-bell.component.html',
  styleUrl: './notification-bell.component.css'
})
export class NotificationBellComponent implements OnInit {
  readonly isOpen = signal(false);
  notifications: NotificationDto[] = [];
  unreadCount = 0;
  latestToast: NotificationDto | null = null;

  constructor(public notificationService: NotificationService) {}

  ngOnInit(): void {
    this.notificationService.notifications$.subscribe(items => {
      this.notifications = items;
    });

    this.notificationService.unreadCount$.subscribe(count => {
      this.unreadCount = count;
    });

    this.notificationService.latestAlert$.subscribe(toast => {
      this.latestToast = toast;
    });
  }

  toggleDropdown(): void {
    this.isOpen.update(v => !v);
  }

  markAsRead(item: NotificationDto): void {
    if (!item.isRead) {
      this.notificationService.markAsRead(item.id).subscribe(() => {
        item.isRead = true;
        this.unreadCount = Math.max(0, this.unreadCount - 1);
      });
    }
  }

  markAllAsRead(): void {
    this.notificationService.markAllAsRead().subscribe(() => {
      this.notifications.forEach(n => n.isRead = true);
      this.unreadCount = 0;
    });
  }

  dismissToast(): void {
    this.notificationService.dismissToast();
  }
}
