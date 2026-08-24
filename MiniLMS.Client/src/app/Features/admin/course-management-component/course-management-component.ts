import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CourseService } from '../../../Core/Services/course.service';
import { AuthService } from '../../../Core/Services/auth-service';
import { Course, CreateLessonDto, CreateSectionDto, Lesson, Section } from '../../../Models/Course';

export interface AdminCourseView extends Course {
  isEditing?: boolean;
}

@Component({
  selector: 'app-course-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './course-management-component.html',
  styleUrls: ['./course-management-component.css']
})
export class CourseManagementComponent implements OnInit {

  courses: AdminCourseView[] = [];
  selectedCourse: Course | null = null;

  // Search & Filters
  searchText = '';

  // Alerts
  successMessage = '';
  errorMessage = '';
  loading = false;

  // Course Form (Create / Edit)
  isEditingCourse = false;
  courseForm = {
    id: 0,
    title: '',
    description: '',
    category: ''
  };

  // Section Form & State
  isAddingSection = false;
  editingSectionId: number | null = null;
  sectionForm: CreateSectionDto = {
    title: '',
    orderIndex: 0
  };

  // Lesson Form & State
  isEditingLesson = false;
  activeSectionForLesson: number | null = null;
  lessonModalOpen = false;
  editingLessonId: number | null = null;
  lessonForm: CreateLessonDto = {
    sectionId: undefined,
    courseId: undefined,
    title: '',
    content: '',
    lessonType: 'Video',
    videoUrl: '',
    durationMinutes: 0,
    isFreePreview: false,
    resourceUrl: '',
    orderIndex: 0
  };

  constructor(
    private courseService: CourseService,
    public authService: AuthService
  ) { }

  ngOnInit(): void {
    this.loadCourses();
  }

  // ===============================
  // Statistics
  // ===============================

  get totalCourses(): number {
    return this.courses.length;
  }

  get publishedCourses(): number {
    return this.courses.filter(x => x.isPublished).length;
  }

  get draftCourses(): number {
    return this.courses.filter(x => !x.isPublished).length;
  }

  get totalLessonsCount(): number {
    return this.courses.reduce((sum, c) => {
      const sectionLessons = c.sections?.reduce((sSum, s) => sSum + (s.lessons?.length || 0), 0) || 0;
      const directLessons = c.lessons?.length || 0;
      return sum + sectionLessons + directLessons;
    }, 0);
  }

  // ===============================
  // Search & Filter
  // ===============================

  filteredCourses(): AdminCourseView[] {
    if (!this.searchText.trim()) {
      return this.courses;
    }

    const query = this.searchText.toLowerCase();
    return this.courses.filter(course =>
      course.title?.toLowerCase().includes(query) ||
      course.category?.toLowerCase().includes(query) ||
      course.description?.toLowerCase().includes(query)
    );
  }

  getCourseLessonsCount(course: Course): number {
    const fromSections = course.sections?.reduce((sum, s) => sum + (s.lessons?.length || 0), 0) || 0;
    const direct = course.lessons?.length || 0;
    return fromSections + direct;
  }

  getCourseSectionsCount(course: Course): number {
    return course.sections?.length || 0;
  }

  // ===============================
  // Alerts
  // ===============================

  showSuccess(message: string): void {
    this.successMessage = message;
    setTimeout(() => {
      this.successMessage = '';
    }, 3500);
  }

  showError(message: string): void {
    this.errorMessage = message;
    setTimeout(() => {
      this.errorMessage = '';
    }, 4500);
  }

  // ===============================
  // Load Courses
  // ===============================

  loadCourses(callback?: () => void): void {
    this.loading = true;

    this.courseService.getAllCourses().subscribe({
      next: (data: Course[]) => {
        this.courses = data.map(c => ({ ...c, isEditing: false }));
        if (this.selectedCourse) {
          const updated = this.courses.find(c => c.id === this.selectedCourse!.id);
          this.selectedCourse = updated || null;
        }
        this.loading = false;
        if (callback) callback();
      },
      error: () => {
        this.loading = false;
        this.showError('Failed to load courses.');
      }
    });
  }

  // ===============================
  // Course CRUD
  // ===============================

  saveCourse(): void {
    if (!this.courseForm.title.trim()) {
      this.showError('Course title is required');
      return;
    }

    if (!this.courseForm.category.trim()) {
      this.showError('Course category is required');
      return;
    }

    if (this.isEditingCourse) {
      this.courseService
        .updateCourse(this.courseForm.id, this.courseForm)
        .subscribe({
          next: () => {
            this.showSuccess('Course updated successfully');
            this.resetCourseForm();
            this.loadCourses();
          },
          error: () => {
            this.showError('Failed to update course');
          }
        });
    } else {
      this.courseService
        .createCourse(this.courseForm)
        .subscribe({
          next: () => {
            this.showSuccess('Course created successfully');
            this.resetCourseForm();
            this.loadCourses();
          },
          error: () => {
            this.showError('Failed to create course');
          }
        });
    }
  }

  editCourse(course: Course): void {
    this.isEditingCourse = true;
    this.courseForm = {
      id: course.id,
      title: course.title,
      description: course.description,
      category: course.category
    };
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  resetCourseForm(): void {
    this.isEditingCourse = false;
    this.courseForm = {
      id: 0,
      title: '',
      description: '',
      category: ''
    };
  }

  deleteCourse(courseId: number): void {
    if (!confirm('Are you sure you want to delete this course? All its sections and lessons will also be deleted.')) {
      return;
    }

    this.courseService.deleteCourse(courseId).subscribe({
      next: () => {
        this.showSuccess('Course deleted successfully');
        if (this.selectedCourse?.id === courseId) {
          this.selectedCourse = null;
        }
        this.loadCourses();
      },
      error: () => {
        this.showError('Failed to delete course');
      }
    });
  }

  publishCourse(courseId: number): void {
    this.courseService.publishCourse(courseId).subscribe({
      next: () => {
        this.showSuccess('Course published successfully!');
        this.loadCourses();
      },
      error: (err) => {
        const msg = err.error?.message || 'Failed to publish course. Ensure it has at least one lesson.';
        this.showError(msg);
      }
    });
  }

  // ===============================
  // Structure & Sections Management
  // ===============================

  manageStructure(course: Course): void {
    this.selectedCourse = course;
    this.isAddingSection = false;
    this.editingSectionId = null;
    this.sectionForm = { title: '', orderIndex: 0 };
    // Scroll down to structure panel
    setTimeout(() => {
      document.getElementById('structure-panel')?.scrollIntoView({ behavior: 'smooth' });
    }, 100);
  }

  closeStructure(): void {
    this.selectedCourse = null;
    this.isAddingSection = false;
    this.editingSectionId = null;
  }

  toggleAddSection(): void {
    this.isAddingSection = !this.isAddingSection;
    this.sectionForm = { title: '', orderIndex: (this.selectedCourse?.sections?.length || 0) + 1 };
  }

  saveSection(): void {
    if (!this.selectedCourse) return;

    if (!this.sectionForm.title.trim()) {
      this.showError('Section title is required');
      return;
    }

    if (this.editingSectionId) {
      this.courseService.updateSection(this.editingSectionId, this.sectionForm).subscribe({
        next: () => {
          this.showSuccess('Section updated successfully');
          this.editingSectionId = null;
          this.sectionForm = { title: '', orderIndex: 0 };
          this.loadCourses();
        },
        error: () => this.showError('Failed to update section')
      });
    } else {
      this.courseService.addSection(this.selectedCourse.id, this.sectionForm).subscribe({
        next: () => {
          this.showSuccess('Section added successfully');
          this.isAddingSection = false;
          this.sectionForm = { title: '', orderIndex: 0 };
          this.loadCourses();
        },
        error: () => this.showError('Failed to add section')
      });
    }
  }

  editSection(section: Section): void {
    this.editingSectionId = section.id;
    this.sectionForm = {
      title: section.title,
      orderIndex: section.orderIndex
    };
  }

  cancelEditSection(): void {
    this.editingSectionId = null;
    this.sectionForm = { title: '', orderIndex: 0 };
  }

  deleteSection(sectionId: number): void {
    if (!confirm('Are you sure you want to delete this section and all its lessons?')) return;

    this.courseService.deleteSection(sectionId).subscribe({
      next: () => {
        this.showSuccess('Section deleted successfully');
        this.loadCourses();
      },
      error: () => this.showError('Failed to delete section')
    });
  }

  moveSectionUp(index: number): void {
    if (!this.selectedCourse?.sections || index === 0) return;
    const list = this.selectedCourse.sections;
    [list[index], list[index - 1]] = [list[index - 1], list[index]];
    this.saveSectionOrder();
  }

  moveSectionDown(index: number): void {
    if (!this.selectedCourse?.sections || index === this.selectedCourse.sections.length - 1) return;
    const list = this.selectedCourse.sections;
    [list[index], list[index + 1]] = [list[index + 1], list[index]];
    this.saveSectionOrder();
  }

  saveSectionOrder(): void {
    if (!this.selectedCourse?.sections) return;
    const sectionIds = this.selectedCourse.sections.map(s => s.id);
    this.courseService.reorderSections(this.selectedCourse.id, sectionIds).subscribe({
      next: () => this.showSuccess('Section order updated'),
      error: () => this.showError('Failed to reorder sections')
    });
  }

  // ===============================
  // Lesson Management (Advanced)
  // ===============================

  openAddLessonModal(sectionId?: number): void {
    this.isEditingLesson = false;
    this.editingLessonId = null;
    this.activeSectionForLesson = sectionId ?? null;

    this.lessonForm = {
      sectionId: sectionId,
      courseId: this.selectedCourse?.id,
      title: '',
      content: '',
      lessonType: 'Video',
      videoUrl: '',
      durationMinutes: 5,
      isFreePreview: false,
      resourceUrl: '',
      orderIndex: 0
    };
    this.lessonModalOpen = true;
  }

  openEditLessonModal(lesson: Lesson, sectionId?: number): void {
    this.isEditingLesson = true;
    this.editingLessonId = lesson.id;
    this.activeSectionForLesson = sectionId ?? lesson.sectionId ?? null;

    this.lessonForm = {
      sectionId: lesson.sectionId ?? sectionId,
      courseId: lesson.courseId ?? this.selectedCourse?.id,
      title: lesson.title,
      content: lesson.content,
      lessonType: lesson.lessonType || 'Video',
      videoUrl: lesson.videoUrl || '',
      durationMinutes: lesson.durationMinutes || 0,
      isFreePreview: !!lesson.isFreePreview,
      resourceUrl: lesson.resourceUrl || '',
      orderIndex: lesson.orderIndex || 0
    };
    this.lessonModalOpen = true;
  }

  closeLessonModal(): void {
    this.lessonModalOpen = false;
    this.editingLessonId = null;
  }

  saveLesson(): void {
    if (!this.lessonForm.title.trim()) {
      this.showError('Lesson title is required');
      return;
    }

    if (this.isEditingLesson && this.editingLessonId) {
      this.courseService.updateLesson(this.editingLessonId, this.lessonForm).subscribe({
        next: () => {
          this.showSuccess('Lesson updated successfully');
          this.closeLessonModal();
          this.loadCourses();
        },
        error: () => this.showError('Failed to update lesson')
      });
    } else {
      if (this.lessonForm.sectionId) {
        this.courseService.addLessonToSection(this.lessonForm.sectionId, this.lessonForm).subscribe({
          next: () => {
            this.showSuccess('Lesson added to section successfully');
            this.closeLessonModal();
            this.loadCourses();
          },
          error: () => this.showError('Failed to add lesson to section')
        });
      } else if (this.selectedCourse) {
        this.courseService.addLesson(this.selectedCourse.id, this.lessonForm).subscribe({
          next: () => {
            this.showSuccess('Lesson added successfully');
            this.closeLessonModal();
            this.loadCourses();
          },
          error: () => this.showError('Failed to add lesson')
        });
      }
    }
  }

  deleteLesson(lessonId: number): void {
    if (!confirm('Are you sure you want to delete this lesson?')) return;

    this.courseService.deleteLesson(lessonId).subscribe({
      next: () => {
        this.showSuccess('Lesson deleted successfully');
        this.loadCourses();
      },
      error: () => this.showError('Failed to delete lesson')
    });
  }

  moveLessonUp(section: Section, index: number): void {
    if (!section.lessons || index === 0) return;
    const list = section.lessons;
    [list[index], list[index - 1]] = [list[index - 1], list[index]];
    this.saveLessonOrderInSection(section);
  }

  moveLessonDown(section: Section, index: number): void {
    if (!section.lessons || index === section.lessons.length - 1) return;
    const list = section.lessons;
    [list[index], list[index + 1]] = [list[index + 1], list[index]];
    this.saveLessonOrderInSection(section);
  }

  saveLessonOrderInSection(section: Section): void {
    const lessonIds = section.lessons.map(l => l.id);
    this.courseService.reorderLessonsInSection(section.id, lessonIds).subscribe({
      next: () => this.showSuccess('Lesson order updated'),
      error: () => this.showError('Failed to reorder lessons')
    });
  }
}