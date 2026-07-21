import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, from, throwError } from 'rxjs';
import { AuthService } from './Services/auth-service';

export const appInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService); // الآن أصبح النوع معروفاً 100%
  const token = authService.getToken();

  // 1. إضافة الـ Token للـ Headers إذا كان موجوداً
  if (token) {
    req = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    });
  }

  // 2. معالجة الأخطاء بشكل صديق للمستخدم
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'حدث خطأ غير متوقع، يرجى المحاولة لاحقاً.';

      if (error.status === 401) {
        errorMessage = 'اسم المستخدم أو كلمة المرور غير صحيحة، أو انتهت جلستك.';
      } else if (error.status === 403) {
        errorMessage = 'غير مصرح لك بدخول هذه الصفحة.';
      } else if (error.error && typeof error.error === 'string') {
        errorMessage = error.error;
      } else if (error.error?.message) {
        errorMessage = error.error.message;
      }

      alert(errorMessage); 
      return throwError(() => new Error(errorMessage));
    })
  );
};