import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from './Services/auth-service';
import { Router } from '@angular/router';

export const appInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const token = authService.getToken();

  // 1. Attach JWT Bearer Token if present
  if (token) {
    req = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    });
  }

  // 2. Handle HTTP Errors
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'An unexpected error occurred. Please try again.';

      if (error.status === 401) {
        // If 401 occurs on a protected route (not login endpoint), log out and redirect
        if (!req.url.includes('/api/Auth/login')) {
          authService.logout();
        }
        errorMessage = 'Invalid username or password, or your session has expired.';
      } else if (error.status === 403) {
        errorMessage = 'You are not authorized to access this resource.';
      } else if (error.error && typeof error.error === 'string') {
        errorMessage = error.error;
      } else if (error.error?.message) {
        errorMessage = error.error.message;
      }

      return throwError(() => ({
        status: error.status,
        message: errorMessage,
        originalError: error
      }));
    })
  );
};