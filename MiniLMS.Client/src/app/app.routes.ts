import { Routes } from '@angular/router';


// 1. استيراد المكونات مباشرة (تأكد من صحة الحروف الكبيرة والصغيرة هنا)
import { LoginComponent } from './Features/auth/login/login-component/login-component'; 
import { SignupComponent } from './Features/auth/signup/signup-component/signup-component';
import { CourseManagementComponent } from './Features/admin/course-management-component/course-management-component'; 
import { DashboardComponent } from './Features/student/dashboard-component/dashboard-component';
import { CourseCatalogComponent } from './Features/student/course-catalog-component/course-catalog-component';
import { CourseClassroomComponent } from './Features/student/course-classroom/course-classroom.component';
import { CertificateVerificationComponent } from './Features/student/certificate-verification/certificate-verification.component';
import { authGuard } from './Core/Guards/auth.guard';

export const routes: Routes = [
  // 1. الصفحات العامة
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  { path: 'login', component: LoginComponent },
  { path: 'signup', component: SignupComponent },
  { path: 'verify-certificate/:code', component: CertificateVerificationComponent },
  
  // 2. منطقة الآدمن المحمية
  { 
    path: 'admin', 
    canActivate: [authGuard], 
    data: { role: 'Admin' },
    children: [
      { path: '', redirectTo: 'courses', pathMatch: 'full' },
      { path: 'courses', component: CourseManagementComponent }
    ]
  },

  // 3. منطقة الطالب المحمية
  { 
    path: 'student', 
    canActivate: [authGuard], 
    data: { role: 'Student' },
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: DashboardComponent },
      { path: 'catalog', component: CourseCatalogComponent },
      { path: 'classroom/:id', component: CourseClassroomComponent }
    ]
  },
  
  // 4. تحويل أي مسار آخر غير معروف إلى تسجيل الدخول
  { path: '**', redirectTo: 'login' }
];