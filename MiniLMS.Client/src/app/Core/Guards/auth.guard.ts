import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../Services/auth-service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isLoggedIn()) {
    router.navigate(['/login']);
    return false;
  }

  const expectedRole = route.data['role'];
  const userRole = authService.getRole();

  if (expectedRole && userRole !== expectedRole) {
    if (userRole === 'Admin') {
      router.navigate(['/admin/courses']);
    } else if (userRole === 'Instructor') {
      router.navigate(['/instructor/dashboard']);
    } else {
      router.navigate(['/student/dashboard']);
    }
    return false;
  }

  return true;
};