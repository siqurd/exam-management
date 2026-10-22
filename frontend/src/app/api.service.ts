import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';

export interface Lesson {
  code: string;
  name: string;
  class: number;
  teacherFirstName: string;
  teacherLastName: string;
}

export interface Student {
  number: number;
  firstName: string;
  lastName: string;
  class: number;
}

export interface ExamResult {
  lessonCode: string;
  studentNumber: number;
  examDate: string;
  grade: number;
}

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);

  getLessons() { return this.http.get<Lesson[]>('/api/lessons'); }
  createLesson(lesson: Lesson) { return this.http.post<Lesson>('/api/lessons', lesson); }
  updateLesson(lesson: Lesson) {
    return this.http.put<void>(`/api/lessons/${encodeURIComponent(lesson.code)}`, lesson);
  }
  deleteLesson(code: string) {
    return this.http.delete<void>(`/api/lessons/${encodeURIComponent(code)}`);
  }

  getStudents() { return this.http.get<Student[]>('/api/students'); }
  createStudent(student: Student) { return this.http.post<Student>('/api/students', student); }
  updateStudent(student: Student) {
    return this.http.put<void>(`/api/students/${student.number}`, student);
  }
  deleteStudent(number: number) { return this.http.delete<void>(`/api/students/${number}`); }

  getExams() { return this.http.get<ExamResult[]>('/api/exams'); }
  createExam(exam: ExamResult) { return this.http.post<ExamResult>('/api/exams', exam); }
  updateExam(exam: ExamResult) { return this.http.put<void>(this.examUrl(exam), exam); }
  deleteExam(exam: ExamResult) { return this.http.delete<void>(this.examUrl(exam)); }

  errorMessage(error: unknown): string {
    if (!(error instanceof HttpErrorResponse)) return 'Xəta baş verdi.';
    if (error.status === 0) return 'Serverə bağlantı mümkün olmadı.';
    if (error.status === 404) return 'Məlumat tapılmadı. Siyahını yeniləyin.';
    const body = error.error;
    if (body?.errors && typeof body.errors === 'object') {
      return Object.values(body.errors).flat().join(' ');
    }
    return body?.title || 'Əməliyyat zamanı xəta baş verdi.';
  }

  private examUrl(exam: ExamResult): string {
    return `/api/exams/${encodeURIComponent(exam.lessonCode)}/${exam.studentNumber}/${exam.examDate}`;
  }
}
