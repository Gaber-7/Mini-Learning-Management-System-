import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { BehaviorSubject, Observable } from 'rxjs';
import * as signalR from '@microsoft/signalr';
import { NotificationDto } from '../../Models/GenAlpha';
import { AuthService } from './auth-service';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private apiUrl = 'https://localhost:7070/api/Notifications';
  private hubUrl = 'https://localhost:7070/hubs/notifications';
  private hubConnection?: signalR.HubConnection;

  private notificationsSubject = new BehaviorSubject<NotificationDto[]>([]);
  public notifications$ = this.notificationsSubject.asObservable();

  private unreadCountSubject = new BehaviorSubject<number>(0);
  public unreadCount$ = this.unreadCountSubject.asObservable();

  private latestAlertSubject = new BehaviorSubject<NotificationDto | null>(null);
  public latestAlert$ = this.latestAlertSubject.asObservable();

  constructor(private http: HttpClient, private authService: AuthService) {
    if (this.authService.isLoggedIn()) {
      this.initSignalR();
      this.loadNotifications();
    }
  }

  private getAuthHeaders(): HttpHeaders {
    const token = this.authService.getToken();
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
  }

  public initSignalR(): void {
    if (this.hubConnection && this.hubConnection.state === signalR.HubConnectionState.Connected) {
      return;
    }

    const token = this.authService.getToken();
    if (!token) return;

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        accessTokenFactory: () => token,
        skipNegotiation: false,
        transport: signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('ReceiveNotification', (notification: NotificationDto) => {
      const current = this.notificationsSubject.value;
      this.notificationsSubject.next([notification, ...current]);
      this.unreadCountSubject.next(this.unreadCountSubject.value + 1);
      this.latestAlertSubject.next(notification);

      // Auto dismiss toast after 5 seconds
      setTimeout(() => {
        if (this.latestAlertSubject.value?.id === notification.id) {
          this.latestAlertSubject.next(null);
        }
      }, 5000);
    });

    this.hubConnection
      .start()
      .then(() => console.log('SignalR Notification Hub Connected.'))
      .catch(err => console.warn('SignalR Connection Warning:', err));
  }

  public stopSignalR(): void {
    if (this.hubConnection) {
      this.hubConnection.stop();
    }
  }

  public loadNotifications(): void {
    this.http.get<NotificationDto[]>(this.apiUrl, { headers: this.getAuthHeaders() })
      .subscribe({
        next: (items) => {
          this.notificationsSubject.next(items);
          const unread = items.filter(i => !i.isRead).length;
          this.unreadCountSubject.next(unread);
        },
        error: (err) => console.warn('Failed to load notifications', err)
      });
  }

  public markAsRead(notificationId: number): Observable<any> {
    return this.http.put(`${this.apiUrl}/${notificationId}/read`, {}, { headers: this.getAuthHeaders() });
  }

  public markAllAsRead(): Observable<any> {
    return this.http.put(`${this.apiUrl}/read-all`, {}, { headers: this.getAuthHeaders() });
  }

  public dismissToast(): void {
    this.latestAlertSubject.next(null);
  }
}
