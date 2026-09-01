import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { CourseService } from '../../../Core/Services/course.service';
import { AuthService } from '../../../Core/Services/auth-service';
import { AdminUsersService, StudentListItem, InstructorListItem, AdminReviewItem } from '../../../Core/Services/admin-users.service';
import { Course, CreateLessonDto, CreateSectionDto, Lesson, Section } from '../../../Models/Course';

export type AdminNavSection =
  | 'dashboard'
  | 'courses'
  | 'categories'
  | 'lessons'
  | 'sections'
  | 'students'
  | 'instructors'
  | 'reviews'
  | 'media'
  | 'settings';

export interface AdminCourseView extends Course {
  isEditing?: boolean;
}

export interface MediaItem {
  id: string;
  name: string;
  type: 'video' | 'pdf' | 'image' | 'archive';
  url: string;
  size: string;
  createdAt: string;
}

@Component({
  selector: 'app-course-management',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './course-management-component.html',
  styleUrls: ['./course-management-component.css']
})
export class CourseManagementComponent implements OnInit {
  // Navigation & UI State
  activeNav: AdminNavSection = 'dashboard';
  sidebarCollapsed = false;
  loading = false;
  successMessage = '';
  errorMessage = '';

  // 1. COURSES STATE
  courses: AdminCourseView[] = [];
  selectedCourse: Course | null = null;
  searchText = '';
  selectedCategoryFilter = '';
  isEditingCourse = false;
  courseModalOpen = false;
  courseForm = {
    id: 0,
    title: '',
    description: '',
    category: 'Development'
  };

  // 2. SECTIONS STATE
  selectedCourseForSections: number | null = null;
  sectionModalOpen = false;
  editingSectionId: number | null = null;
  sectionForm: CreateSectionDto = {
    title: '',
    orderIndex: 1
  };

  // 3. LESSONS STATE
  selectedCourseForLessons: number | null = null;
  selectedSectionForLessons: number | null = null;
  lessonModalOpen = false;
  editingLessonId: number | null = null;
  previewVideoUrl: SafeResourceUrl | null = null;
  previewVideoTitle = '';
  lessonForm: CreateLessonDto = {
    sectionId: undefined,
    courseId: undefined,
    title: '',
    content: '',
    lessonType: 'Video',
    videoUrl: '',
    durationMinutes: 10,
    isFreePreview: false,
    resourceUrl: '',
    orderIndex: 1
  };

  // 4. STUDENTS STATE
  students: StudentListItem[] = [];
  studentSearchText = '';
  studentModalOpen = false;
  editingStudentId: number | null = null;
  studentForm = {
    username: '',
    fullName: '',
    email: '',
    password: ''
  };

  // 5. INSTRUCTORS STATE
  instructors: InstructorListItem[] = [];
  instructorSearchText = '';
  instructorModalOpen = false;
  editingInstructorId: number | null = null;
  instructorForm = {
    username: '',
    fullName: '',
    email: '',
    password: '',
    headline: '',
    bio: '',
    profilePictureUrl: '',
    websiteUrl: '',
    linkedInUrl: '',
    gitHubUrl: '',
    youTubeUrl: ''
  };

  // 6. CATEGORIES STATE
  categories: string[] = ['Development', 'Design', 'Business', 'Marketing', 'Data Science', 'Cybersecurity', 'Cloud Computing'];
  categorySearchText = '';
  categoryModalOpen = false;
  editingCategoryName: string | null = null;
  categoryInputName = '';

  // 7. REVIEWS STATE
  reviews: AdminReviewItem[] = [];
  reviewSearchText = '';

  // 8. MEDIA LIBRARY STATE
  mediaSearchText = '';
  mediaModalOpen = false;
  mediaForm: MediaItem = {
    id: '',
    name: '',
    type: 'video',
    url: '',
    size: '15.4 MB',
    createdAt: new Date().toISOString()
  };
  mediaItems: MediaItem[] = [
    { id: '1', name: 'Angular 19 Complete Guide.mp4', type: 'video', url: 'https://www.youtube.com/watch?v=k5E2AVpwsko', size: '245 MB', createdAt: '2026-08-20' },
    { id: '2', name: 'Clean Architecture .NET 9.pdf', type: 'pdf', url: 'https://example.com/clean-arch.pdf', size: '4.2 MB', createdAt: '2026-08-22' },
    { id: '3', name: 'Course Thumbnail Starter.png', type: 'image', url: 'https://images.unsplash.com/photo-1516321318423-f06f85e504b3', size: '1.8 MB', createdAt: '2026-08-23' },
    { id: '4', name: 'Project Source Code v1.zip', type: 'archive', url: 'https://github.com/project.zip', size: '18.5 MB', createdAt: '2026-08-24' }
  ];

  // 9. SETTINGS STATE
  platformSettings = {
    siteName: 'MiniLMS Enterprise Platform',
    supportEmail: 'support@minilms.com',
    allowStudentRegistration: true,
    requireCourseApproval: true,
    currency: 'USD ($)',
    maintenanceMode: false
  };

  // Delete Confirmation Modal
  confirmModalOpen = false;
  confirmTitle = '';
  confirmMessage = '';
  confirmAction: (() => void) | null = null;

  constructor(
    private courseService: CourseService,
    private adminUsersService: AdminUsersService,
    public authService: AuthService,
    private sanitizer: DomSanitizer
  ) {}

  ngOnInit(): void {
    this.loadAllData();
  }

  loadAllData(): void {
    this.loadCourses();
    this.loadStudents();
    this.loadInstructors();
    this.loadReviews();
  }

  // ===============================
  // NAVIGATION
  // ===============================

  setNav(section: AdminNavSection): void {
    this.activeNav = section;
    this.clearAlerts();
  }

  toggleSidebar(): void {
    this.sidebarCollapsed = !this.sidebarCollapsed;
  }

  // ===============================
  // 1. COURSES LOGIC
  // ===============================

  loadCourses(): void {
    this.loading = true;
    this.courseService.getAllCourses().subscribe({
      next: (data: Course[]) => {
        this.courses = data;
        this.loading = false;
        if (!this.selectedCourseForSections && data.length > 0) {
          this.selectedCourseForSections = data[0].id;
        }
        if (!this.selectedCourseForLessons && data.length > 0) {
          this.selectedCourseForLessons = data[0].id;
        }
      },
      error: () => {
        this.loading = false;
        this.showError('Failed to load courses.');
      }
    });
  }

  get filteredCourses(): AdminCourseView[] {
    return this.courses.filter(c => {
      const matchText = !this.searchText || c.title.toLowerCase().includes(this.searchText.toLowerCase()) || c.category.toLowerCase().includes(this.searchText.toLowerCase());
      const matchCat = !this.selectedCategoryFilter || c.category === this.selectedCategoryFilter;
      return matchText && matchCat;
    });
  }

  openCreateCourseModal(): void {
    this.isEditingCourse = false;
    this.courseForm = { id: 0, title: '', description: '', category: this.categories[0] || 'Development' };
    this.courseModalOpen = true;
  }

  openEditCourseModal(course: Course): void {
    this.isEditingCourse = true;
    this.courseForm = {
      id: course.id,
      title: course.title,
      description: course.description,
      category: course.category
    };
    this.courseModalOpen = true;
  }

  saveCourse(): void {
    if (!this.courseForm.title.trim() || !this.courseForm.category) {
      this.showError('Please provide course title and category.');
      return;
    }

    if (this.isEditingCourse) {
      this.courseService.updateCourse(this.courseForm.id, this.courseForm).subscribe({
        next: () => {
          this.showSuccess('Course updated successfully.');
          this.courseModalOpen = false;
          this.loadCourses();
        },
        error: () => this.showError('Failed to update course.')
      });
    } else {
      this.courseService.createCourse(this.courseForm).subscribe({
        next: () => {
          this.showSuccess('Course created successfully.');
          this.courseModalOpen = false;
          this.loadCourses();
        },
        error: () => this.showError('Failed to create course.')
      });
    }
  }

  confirmDeleteCourse(course: Course): void {
    this.confirmTitle = 'Delete Course';
    this.confirmMessage = `Are you sure you want to permanently delete course "${course.title}"? All sections, lessons, quizzes, and reviews will be removed.`;
    this.confirmAction = () => {
      this.courseService.deleteCourse(course.id).subscribe({
        next: () => {
          this.showSuccess('Course deleted successfully.');
          this.loadCourses();
        },
        error: () => this.showError('Failed to delete course.')
      });
    };
    this.confirmModalOpen = true;
  }

  togglePublish(course: Course): void {
    const updated = { ...course, isPublished: !course.isPublished };
    this.courseService.updateCourse(course.id, updated).subscribe({
      next: () => {
        course.isPublished = !course.isPublished;
        this.showSuccess(course.isPublished ? 'Course published!' : 'Course unpublished (Draft).');
      },
      error: () => this.showError('Failed to change publishing status.')
    });
  }

  // ===============================
  // 2. SECTIONS LOGIC
  // ===============================

  get currentSectionsCourse(): AdminCourseView | undefined {
    return this.courses.find(c => c.id === Number(this.selectedCourseForSections));
  }

  openCreateSectionModal(): void {
    if (!this.selectedCourseForSections) {
      this.showError('Please select a course first.');
      return;
    }
    this.editingSectionId = null;
    this.sectionForm = {
      title: '',
      orderIndex: (this.currentSectionsCourse?.sections?.length || 0) + 1
    };
    this.sectionModalOpen = true;
  }

  openEditSectionModal(sec: Section): void {
    this.editingSectionId = sec.id;
    this.sectionForm = {
      title: sec.title,
      orderIndex: sec.orderIndex
    };
    this.sectionModalOpen = true;
  }

  saveSection(): void {
    if (!this.selectedCourseForSections || !this.sectionForm.title.trim()) {
      this.showError('Please provide section title.');
      return;
    }

    if (this.editingSectionId) {
      this.courseService.updateSection(this.editingSectionId, this.sectionForm).subscribe({
        next: () => {
          this.showSuccess('Section updated successfully.');
          this.sectionModalOpen = false;
          this.loadCourses();
        },
        error: () => this.showError('Failed to update section.')
      });
    } else {
      this.courseService.addSection(Number(this.selectedCourseForSections), this.sectionForm).subscribe({
        next: () => {
          this.showSuccess('Section created successfully.');
          this.sectionModalOpen = false;
          this.loadCourses();
        },
        error: () => this.showError('Failed to create section.')
      });
    }
  }

  confirmDeleteSection(sec: Section): void {
    this.confirmTitle = 'Delete Section';
    this.confirmMessage = `Are you sure you want to delete section "${sec.title}" and all its lessons?`;
    this.confirmAction = () => {
      this.courseService.deleteSection(sec.id).subscribe({
        next: () => {
          this.showSuccess('Section deleted successfully.');
          this.loadCourses();
        },
        error: () => this.showError('Failed to delete section.')
      });
    };
    this.confirmModalOpen = true;
  }

  // ===============================
  // 3. LESSONS LOGIC
  // ===============================

  get currentLessonsCourse(): AdminCourseView | undefined {
    return this.courses.find(c => c.id === Number(this.selectedCourseForLessons));
  }

  get allLessonsForSelectedCourse(): Lesson[] {
    const c = this.currentLessonsCourse;
    if (!c) return [];
    const fromSections = c.sections?.flatMap(s => s.lessons || []) || [];
    const direct = c.lessons || [];
    return [...fromSections, ...direct];
  }

  openCreateLessonModal(): void {
    if (!this.selectedCourseForLessons) {
      this.showError('Please select a course first.');
      return;
    }
    const c = this.currentLessonsCourse;
    this.editingLessonId = null;
    this.lessonForm = {
      courseId: Number(this.selectedCourseForLessons),
      sectionId: c?.sections?.[0]?.id || undefined,
      title: '',
      content: '',
      lessonType: 'Video',
      videoUrl: '',
      durationMinutes: 10,
      isFreePreview: false,
      resourceUrl: '',
      orderIndex: (this.allLessonsForSelectedCourse.length || 0) + 1
    };
    this.lessonModalOpen = true;
  }

  openEditLessonModal(les: Lesson): void {
    this.editingLessonId = les.id;
    this.lessonForm = {
      courseId: les.courseId || (this.selectedCourseForLessons ? Number(this.selectedCourseForLessons) : undefined),
      sectionId: les.sectionId || undefined,
      title: les.title,
      content: les.content || '',
      lessonType: les.lessonType || 'Video',
      videoUrl: les.videoUrl || '',
      durationMinutes: les.durationMinutes || 0,
      isFreePreview: les.isFreePreview || false,
      resourceUrl: les.resourceUrl || '',
      orderIndex: les.orderIndex || 1
    };
    this.lessonModalOpen = true;
  }

  saveLesson(): void {
    if (!this.lessonForm.title.trim()) {
      this.showError('Please enter a lesson title.');
      return;
    }

    if (this.editingLessonId) {
      this.courseService.updateLesson(this.editingLessonId, this.lessonForm).subscribe({
        next: () => {
          this.showSuccess('Lesson updated successfully.');
          this.lessonModalOpen = false;
          this.loadCourses();
        },
        error: () => this.showError('Failed to update lesson.')
      });
    } else {
      if (this.lessonForm.sectionId) {
        this.courseService.addLessonToSection(this.lessonForm.sectionId, this.lessonForm).subscribe({
          next: () => {
            this.showSuccess('Lesson added to section successfully.');
            this.lessonModalOpen = false;
            this.loadCourses();
          },
          error: () => this.showError('Failed to add lesson.')
        });
      } else if (this.selectedCourseForLessons) {
        this.courseService.addLesson(Number(this.selectedCourseForLessons), this.lessonForm).subscribe({
          next: () => {
            this.showSuccess('Lesson created successfully.');
            this.lessonModalOpen = false;
            this.loadCourses();
          },
          error: () => this.showError('Failed to create lesson.')
        });
      }
    }
  }

  confirmDeleteLesson(les: Lesson): void {
    this.confirmTitle = 'Delete Lesson';
    this.confirmMessage = `Are you sure you want to delete lesson "${les.title}"?`;
    this.confirmAction = () => {
      this.courseService.deleteLesson(les.id).subscribe({
        next: () => {
          this.showSuccess('Lesson deleted successfully.');
          this.loadCourses();
        },
        error: () => this.showError('Failed to delete lesson.')
      });
    };
    this.confirmModalOpen = true;
  }

  openVideoPreview(lesson: Lesson): void {
    this.previewVideoTitle = lesson.title;
    if (lesson.videoUrl) {
      let url = lesson.videoUrl;
      if (url.includes('youtube.com/watch?v=')) url = url.replace('watch?v=', 'embed/');
      else if (url.includes('youtu.be/')) url = url.replace('youtu.be/', 'youtube.com/embed/');
      this.previewVideoUrl = this.sanitizer.bypassSecurityTrustResourceUrl(url);
    } else {
      this.previewVideoUrl = null;
    }
  }

  closeVideoPreview(): void {
    this.previewVideoUrl = null;
  }

  // ===============================
  // 4. STUDENTS LOGIC
  // ===============================

  loadStudents(): void {
    this.adminUsersService.getStudents().subscribe({
      next: (data) => (this.students = data),
      error: () => this.showError('Failed to load students.')
    });
  }

  get filteredStudents(): StudentListItem[] {
    if (!this.studentSearchText) return this.students;
    const term = this.studentSearchText.toLowerCase();
    return this.students.filter(s =>
      s.fullName.toLowerCase().includes(term) ||
      s.username.toLowerCase().includes(term) ||
      s.email.toLowerCase().includes(term)
    );
  }

  openCreateStudentModal(): void {
    this.editingStudentId = null;
    this.studentForm = { username: '', fullName: '', email: '', password: '' };
    this.studentModalOpen = true;
  }

  openEditStudentModal(student: StudentListItem): void {
    this.editingStudentId = student.id;
    this.studentForm = {
      username: student.username,
      fullName: student.fullName,
      email: student.email,
      password: ''
    };
    this.studentModalOpen = true;
  }

  saveStudent(): void {
    if (!this.studentForm.fullName.trim() || !this.studentForm.email.trim()) {
      this.showError('Full name and email are required.');
      return;
    }

    if (this.editingStudentId) {
      this.adminUsersService.updateStudent(this.editingStudentId, {
        fullName: this.studentForm.fullName,
        email: this.studentForm.email,
        password: this.studentForm.password || undefined
      }).subscribe({
        next: () => {
          this.showSuccess('Student updated successfully.');
          this.studentModalOpen = false;
          this.loadStudents();
        },
        error: () => this.showError('Failed to update student.')
      });
    } else {
      if (!this.studentForm.username.trim() || !this.studentForm.password.trim()) {
        this.showError('Username and password are required for new students.');
        return;
      }
      this.adminUsersService.createStudent(this.studentForm).subscribe({
        next: () => {
          this.showSuccess('Student created successfully.');
          this.studentModalOpen = false;
          this.loadStudents();
        },
        error: (err) => this.showError(err.error?.message || 'Failed to create student.')
      });
    }
  }

  confirmDeleteStudent(student: StudentListItem): void {
    this.confirmTitle = 'Delete Student';
    this.confirmMessage = `Are you sure you want to delete student "${student.fullName}" (@${student.username})? All their enrollments and quiz attempts will be removed.`;
    this.confirmAction = () => {
      this.adminUsersService.deleteStudent(student.id).subscribe({
        next: () => {
          this.showSuccess('Student deleted successfully.');
          this.loadStudents();
        },
        error: () => this.showError('Failed to delete student.')
      });
    };
    this.confirmModalOpen = true;
  }

  // ===============================
  // 5. INSTRUCTORS LOGIC
  // ===============================

  loadInstructors(): void {
    this.adminUsersService.getInstructors().subscribe({
      next: (data) => (this.instructors = data),
      error: () => this.showError('Failed to load instructors.')
    });
  }

  get filteredInstructors(): InstructorListItem[] {
    if (!this.instructorSearchText) return this.instructors;
    const term = this.instructorSearchText.toLowerCase();
    return this.instructors.filter(i =>
      i.fullName.toLowerCase().includes(term) ||
      i.username.toLowerCase().includes(term) ||
      i.email.toLowerCase().includes(term) ||
      (i.headline && i.headline.toLowerCase().includes(term))
    );
  }

  openCreateInstructorModal(): void {
    this.editingInstructorId = null;
    this.instructorForm = {
      username: '',
      fullName: '',
      email: '',
      password: '',
      headline: '',
      bio: '',
      profilePictureUrl: '',
      websiteUrl: '',
      linkedInUrl: '',
      gitHubUrl: '',
      youTubeUrl: ''
    };
    this.instructorModalOpen = true;
  }

  openEditInstructorModal(instructor: InstructorListItem): void {
    this.editingInstructorId = instructor.id;
    this.instructorForm = {
      username: instructor.username,
      fullName: instructor.fullName,
      email: instructor.email,
      password: '',
      headline: instructor.headline || '',
      bio: instructor.bio || '',
      profilePictureUrl: instructor.profilePictureUrl || '',
      websiteUrl: instructor.websiteUrl || '',
      linkedInUrl: instructor.linkedInUrl || '',
      gitHubUrl: instructor.gitHubUrl || '',
      youTubeUrl: instructor.youTubeUrl || ''
    };
    this.instructorModalOpen = true;
  }

  saveInstructor(): void {
    if (!this.instructorForm.fullName.trim() || !this.instructorForm.email.trim()) {
      this.showError('Full name and email are required.');
      return;
    }

    if (this.editingInstructorId) {
      this.adminUsersService.updateInstructor(this.editingInstructorId, {
        fullName: this.instructorForm.fullName,
        email: this.instructorForm.email,
        password: this.instructorForm.password || undefined,
        headline: this.instructorForm.headline,
        bio: this.instructorForm.bio,
        profilePictureUrl: this.instructorForm.profilePictureUrl,
        websiteUrl: this.instructorForm.websiteUrl,
        linkedInUrl: this.instructorForm.linkedInUrl,
        gitHubUrl: this.instructorForm.gitHubUrl,
        youTubeUrl: this.instructorForm.youTubeUrl
      }).subscribe({
        next: () => {
          this.showSuccess('Instructor updated successfully.');
          this.instructorModalOpen = false;
          this.loadInstructors();
        },
        error: () => this.showError('Failed to update instructor.')
      });
    } else {
      if (!this.instructorForm.username.trim() || !this.instructorForm.password.trim()) {
        this.showError('Username and password are required for new instructors.');
        return;
      }
      this.adminUsersService.createInstructor(this.instructorForm).subscribe({
        next: () => {
          this.showSuccess('Instructor created successfully.');
          this.instructorModalOpen = false;
          this.loadInstructors();
        },
        error: (err) => this.showError(err.error?.message || 'Failed to create instructor.')
      });
    }
  }

  confirmDeleteInstructor(instructor: InstructorListItem): void {
    this.confirmTitle = 'Delete Instructor';
    this.confirmMessage = `Are you sure you want to delete instructor "${instructor.fullName}" (@${instructor.username})?`;
    this.confirmAction = () => {
      this.adminUsersService.deleteInstructor(instructor.id).subscribe({
        next: () => {
          this.showSuccess('Instructor deleted successfully.');
          this.loadInstructors();
        },
        error: () => this.showError('Failed to delete instructor.')
      });
    };
    this.confirmModalOpen = true;
  }

  // ===============================
  // 6. CATEGORIES LOGIC
  // ===============================

  get filteredCategories(): string[] {
    if (!this.categorySearchText) return this.categories;
    return this.categories.filter(c => c.toLowerCase().includes(this.categorySearchText.toLowerCase()));
  }

  openCreateCategoryModal(): void {
    this.editingCategoryName = null;
    this.categoryInputName = '';
    this.categoryModalOpen = true;
  }

  openEditCategoryModal(cat: string): void {
    this.editingCategoryName = cat;
    this.categoryInputName = cat;
    this.categoryModalOpen = true;
  }

  saveCategory(): void {
    const val = this.categoryInputName.trim();
    if (!val) {
      this.showError('Category name is required.');
      return;
    }

    if (this.editingCategoryName) {
      const idx = this.categories.indexOf(this.editingCategoryName);
      if (idx !== -1) this.categories[idx] = val;
      this.showSuccess('Category renamed successfully.');
    } else {
      if (!this.categories.includes(val)) {
        this.categories.push(val);
        this.showSuccess('Category added successfully.');
      }
    }
    this.categoryModalOpen = false;
  }

  confirmDeleteCategory(cat: string): void {
    this.confirmTitle = 'Delete Category';
    this.confirmMessage = `Are you sure you want to remove category "${cat}"?`;
    this.confirmAction = () => {
      this.categories = this.categories.filter(c => c !== cat);
      this.showSuccess('Category deleted.');
    };
    this.confirmModalOpen = true;
  }

  getCategoryCoursesCount(cat: string): number {
    return this.courses.filter(c => c.category === cat).length;
  }

  // ===============================
  // 7. REVIEWS LOGIC
  // ===============================

  loadReviews(): void {
    this.adminUsersService.getReviews().subscribe({
      next: (data) => (this.reviews = data),
      error: () => this.showError('Failed to load reviews.')
    });
  }

  get filteredReviews(): AdminReviewItem[] {
    if (!this.reviewSearchText) return this.reviews;
    const term = this.reviewSearchText.toLowerCase();
    return this.reviews.filter(r =>
      r.studentName.toLowerCase().includes(term) ||
      r.courseTitle.toLowerCase().includes(term) ||
      r.comment.toLowerCase().includes(term)
    );
  }

  toggleReviewApproval(review: AdminReviewItem): void {
    this.adminUsersService.toggleReviewApproval(review.id).subscribe({
      next: () => {
        review.isApproved = !review.isApproved;
        this.showSuccess(`Review ${review.isApproved ? 'Approved' : 'Hidden'} successfully.`);
      },
      error: () => this.showError('Failed to update review status.')
    });
  }

  confirmDeleteReview(review: AdminReviewItem): void {
    this.confirmTitle = 'Delete Review';
    this.confirmMessage = `Are you sure you want to delete review by "${review.studentName}" on course "${review.courseTitle}"?`;
    this.confirmAction = () => {
      this.adminUsersService.deleteReview(review.id).subscribe({
        next: () => {
          this.reviews = this.reviews.filter(r => r.id !== review.id);
          this.showSuccess('Review deleted successfully.');
        },
        error: () => this.showError('Failed to delete review.')
      });
    };
    this.confirmModalOpen = true;
  }

  // ===============================
  // 8. MEDIA LIBRARY LOGIC
  // ===============================

  get filteredMedia(): MediaItem[] {
    if (!this.mediaSearchText) return this.mediaItems;
    const term = this.mediaSearchText.toLowerCase();
    return this.mediaItems.filter(m => m.name.toLowerCase().includes(term) || m.type.toLowerCase().includes(term));
  }

  openAddMediaModal(): void {
    this.mediaForm = {
      id: (this.mediaItems.length + 1).toString(),
      name: '',
      type: 'video',
      url: '',
      size: '12.0 MB',
      createdAt: new Date().toISOString().split('T')[0]
    };
    this.mediaModalOpen = true;
  }

  saveMedia(): void {
    if (!this.mediaForm.name.trim() || !this.mediaForm.url.trim()) {
      this.showError('Media name and URL are required.');
      return;
    }
    this.mediaItems.unshift({ ...this.mediaForm });
    this.showSuccess('Media asset added to library.');
    this.mediaModalOpen = false;
  }

  deleteMedia(item: MediaItem): void {
    this.mediaItems = this.mediaItems.filter(m => m.id !== item.id);
    this.showSuccess('Media asset removed.');
  }

  copyToClipboard(text: string): void {
    navigator.clipboard.writeText(text).then(() => {
      this.showSuccess('URL copied to clipboard!');
    });
  }

  // ===============================
  // 9. SETTINGS LOGIC
  // ===============================

  saveSettings(): void {
    this.showSuccess('Platform settings saved successfully.');
  }

  // ===============================
  // HELPER METRICS & ALERTS
  // ===============================

  get totalCourses(): number { return this.courses.length; }
  get publishedCourses(): number { return this.courses.filter(x => x.isPublished).length; }
  get draftCourses(): number { return this.courses.filter(x => !x.isPublished).length; }
  get totalStudentsCount(): number { return this.students.length; }
  get totalInstructorsCount(): number { return this.instructors.length; }
  get totalReviewsCount(): number { return this.reviews.length; }

  get totalLessonsCount(): number {
    return this.courses.reduce((sum, c) => {
      const sectionLessons = c.sections?.reduce((sSum, s) => sSum + (s.lessons?.length || 0), 0) || 0;
      const directLessons = c.lessons?.length || 0;
      return sum + sectionLessons + directLessons;
    }, 0);
  }

  executeConfirm(): void {
    if (this.confirmAction) {
      this.confirmAction();
      this.confirmAction = null;
    }
    this.confirmModalOpen = false;
  }

  showSuccess(msg: string): void {
    this.successMessage = msg;
    this.errorMessage = '';
    setTimeout(() => (this.successMessage = ''), 4000);
  }

  showError(msg: string): void {
    this.errorMessage = msg;
    this.successMessage = '';
    setTimeout(() => (this.errorMessage = ''), 5000);
  }

  clearAlerts(): void {
    this.successMessage = '';
    this.errorMessage = '';
  }
}