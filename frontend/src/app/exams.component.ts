import { Component, OnInit, inject, signal } from '@angular/core';
import { DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Observable, finalize, forkJoin } from 'rxjs';
import { ApiService, ExamResult, Lesson, Student } from './api.service';

@Component({
  selector: 'app-exams',
  imports: [FormsModule, DatePipe],
  template: `
    <h2>İmtahanlar</h2>
    @if (error()) { <p class="error" role="alert">{{ error() }}</p> }
    <form #form="ngForm" (ngSubmit)="save()">
      <h3>{{ editing ? 'Qiyməti redaktə et' : 'Yeni imtahan' }}</h3>
      @if (!loading() && (!lessons().length || !students().length)) {
        <p class="hint">Əvvəlcə dərs və şagird qeydiyyatını tamamlayın.</p>
      }
      <div class="fields">
        <label>Dərs
          <select name="lessonCode" [(ngModel)]="model.lessonCode" required [disabled]="editing || busy()">
            <option value="">Dərs seçin</option>
            @for (lesson of lessons(); track lesson.code) {
              <option [value]="lesson.code">{{ lesson.code }} — {{ lesson.name }} ({{ lesson.class }}-ci sinif)</option>
            }
          </select>
        </label>
        <label>Şagird
          <select name="studentNumber" [(ngModel)]="model.studentNumber" required min="1" [disabled]="editing || busy()">
            <option [ngValue]="0">Şagird seçin</option>
            @for (student of students(); track student.number) {
              <option [ngValue]="student.number">{{ student.number }} — {{ student.firstName }} {{ student.lastName }} ({{ student.class }}-ci sinif)</option>
            }
          </select>
        </label>
        <label>İmtahan tarixi
          <input type="date" name="examDate" [(ngModel)]="model.examDate" required [readonly]="editing || busy()">
        </label>
        <label>Qiymət
          <input type="number" name="grade" [(ngModel)]="model.grade" required min="0" max="9" step="1" [readonly]="busy()">
        </label>
      </div>
      <div class="actions">
        <button type="submit" [disabled]="form.invalid || !model.lessonCode || !model.studentNumber || busy() || loading()">{{ busy() ? 'Gözləyin…' : 'Yadda saxla' }}</button>
        <button type="button" class="secondary" (click)="reset(); form.resetForm(model)" [disabled]="busy()">Təmizlə</button>
      </div>
    </form>
    <button type="button" class="secondary" (click)="load()" [disabled]="loading() || busy()">Siyahını yenilə</button>
    @if (loading()) { <p role="status">Yüklənir…</p> }
    <div class="table-wrap">
      <table>
        <thead><tr><th>Dərs</th><th>Şagird</th><th>Tarix</th><th>Qiymət</th><th>Əməliyyat</th></tr></thead>
        <tbody>
          @for (exam of exams(); track exam.lessonCode + '/' + exam.studentNumber + '/' + exam.examDate) {
            <tr>
              <td>{{ exam.lessonCode }} — {{ lessonName(exam.lessonCode) }}</td>
              <td>{{ exam.studentNumber }} — {{ studentName(exam.studentNumber) }}</td>
              <td>{{ exam.examDate | date:'dd.MM.yyyy' }}</td><td>{{ exam.grade }}</td>
              <td><div class="actions">
                <button type="button" class="secondary" (click)="edit(exam)" [disabled]="busy() || loading()">Redaktə</button>
                <button type="button" class="danger" (click)="remove(exam)" [disabled]="busy() || loading()">Sil</button>
              </div></td>
            </tr>
          } @empty {
            @if (!loading()) { <tr><td colspan="5">İmtahan qeyd olunmayıb.</td></tr> }
          }
        </tbody>
      </table>
    </div>
  `
})
export class ExamsComponent implements OnInit {
  private readonly api = inject(ApiService);
  readonly exams = signal<ExamResult[]>([]);
  readonly lessons = signal<Lesson[]>([]);
  readonly students = signal<Student[]>([]);
  readonly loading = signal(false);
  readonly busy = signal(false);
  readonly error = signal('');
  editing = false;
  model = this.empty();

  ngOnInit() { this.load(); }

  load() {
    this.loading.set(true);
    this.error.set('');
    forkJoin({ exams: this.api.getExams(), lessons: this.api.getLessons(), students: this.api.getStudents() })
      .pipe(finalize(() => this.loading.set(false))).subscribe({
        next: data => {
          this.exams.set(data.exams);
          this.lessons.set(data.lessons);
          this.students.set(data.students);
        },
        error: error => this.error.set(this.api.errorMessage(error))
      });
  }

  edit(exam: ExamResult) {
    this.model = { ...exam };
    this.editing = true;
    this.error.set('');
  }

  save() {
    if (this.busy() || this.loading()) return;
    this.busy.set(true);
    this.error.set('');
    const request: Observable<unknown> = this.editing ? this.api.updateExam(this.model) : this.api.createExam(this.model);
    request.pipe(finalize(() => this.busy.set(false))).subscribe({
      next: () => { this.reset(); this.load(); },
      error: error => this.error.set(this.api.errorMessage(error))
    });
  }

  remove(exam: ExamResult) {
    if (!window.confirm('İmtahan nəticəsi silinsin?')) return;
    this.busy.set(true);
    this.error.set('');
    this.api.deleteExam(exam).pipe(finalize(() => this.busy.set(false))).subscribe({
      next: () => {
        if (this.model.lessonCode === exam.lessonCode && this.model.studentNumber === exam.studentNumber
            && this.model.examDate === exam.examDate) this.reset();
        this.load();
      },
      error: error => this.error.set(this.api.errorMessage(error))
    });
  }

  lessonName(code: string) { return this.lessons().find(lesson => lesson.code === code)?.name ?? ''; }
  studentName(number: number) {
    const student = this.students().find(item => item.number === number);
    return student ? `${student.firstName} ${student.lastName}` : '';
  }

  reset() { this.model = this.empty(); this.editing = false; }
  private empty(): ExamResult { return { lessonCode: '', studentNumber: 0, examDate: '', grade: 0 }; }
}
