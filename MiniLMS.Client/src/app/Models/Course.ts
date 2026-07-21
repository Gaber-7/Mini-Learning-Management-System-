export interface Lesson {
  id: number;
  courseId: number;
  title: string;
  content: string;
  orderIndex: number;
}

export interface Course {
  id: number;
  title: string;
  description: string;
  category: string;
  isPublished: boolean;
  lessons: Lesson[];
}