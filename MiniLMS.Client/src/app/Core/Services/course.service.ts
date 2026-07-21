import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CourseService {
  private apiUrl = 'https://localhost:7070/api/Courses';

  constructor(private http: HttpClient) {}

  // 1. كتالوج الكورسات المتاحة
  getPublishedCourses(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/published`);
  }

  // 2. التسجيل في كورس
  enrollInCourse(courseId: number): Observable<any> {
    return this.http.post(`${this.apiUrl}/${courseId}/enroll`, {});
  }

  // 3. الكورسات المسجل بها الطالب (Dashboard)
  getEnrolledCourses(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/enrolled`);
  }

  // 4. تحديث حالة الدرس (مكتمل أو غير مكتمل)
  toggleLessonCompletion(courseId: number, lessonId: number, isCompleted: boolean): Observable<any> {
    return this.http.post(`${this.apiUrl}/${courseId}/lessons/${lessonId}/complete`, { isCompleted });
  }

  getAllCourses(): Observable<any[]> {
  return this.http.get<any[]>(`${this.apiUrl}`);
}

// 2. إنشاء كورس جديد
createCourse(courseData: any): Observable<any> {
  return this.http.post(`${this.apiUrl}`, courseData);
}

// 3. تعديل كورس الحالي
updateCourse(courseId: number, courseData: any): Observable<any> {
  return this.http.put(`${this.apiUrl}/${courseId}`, courseData);
}

// 4. نشر الكورس (Publish Action)
publishCourse(courseId: number): Observable<any> {
  return this.http.post(`${this.apiUrl}/${courseId}/publish`, {});
}

// 5. إدارة الدروس (إضافة درس جديد)
addLesson(courseId: number, lessonData: any): Observable<any> {
  return this.http.post(`${this.apiUrl}/${courseId}/lessons`, lessonData);
}

// 6. حذف درس معين
deleteLesson(courseId: number, lessonId: number): Observable<any> {
  return this.http.delete(`${this.apiUrl}/${courseId}/lessons/${lessonId}`);
}

// 7. جلب الطلاب المسجلين في كورس معين ونسبة تقدمهم
getCourseStudentsProgress(courseId: number): Observable<any[]> {
  return this.http.get<any[]>(`${this.apiUrl}/${courseId}/students-progress`);
}
}