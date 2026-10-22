import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Observable, finalize } from 'rxjs';
import { ApiService, Lesson } from './api.service';

@Component({
  selector: 'app-lessons',
  imports: [FormsModule],
  template: `
    <h2>Dərslər</h2>
    @if (error()) { <p class="error" role="alert">{{ error() }}</p> }
    <form #form="ngForm" (ngSubmit)="save()">
      <h3>{{ editing ? 'Dərsi redaktə et' : 'Yeni dərs' }}</h3>
      <div class="fields">
        <label>Dərsin kodu
          <input name="code" [(ngModel)]="model.code" required minlength="3" maxlength="3" [readonly]="editing || busy()">
        </label>
        <label>Dərsin adı
          <input name="name" [(ngModel)]="model.name" required maxlength="30" [readonly]="busy()">
        </label>
        <label>Sinif
          <input type="number" name="class" [(ngModel)]="model.class" required min="1" max="99" step="1" [readonly]="busy()">
        </label>
        <label>Müəllimin adı
          <input name="teacherFirstName" [(ngModel)]="model.teacherFirstName" required maxlength="20" [readonly]="busy()">
        </label>
        <label>Müəllimin soyadı
          <input name="teacherLastName" [(ngModel)]="model.teacherLastName" required maxlength="20" [readonly]="busy()">
        </label>
      </div>
      <div class="actions">
        <button type="submit" [disabled]="form.invalid || busy() || loading()">{{ busy() ? 'Gözləyin…' : 'Yadda saxla' }}</button>
        <button type="button" class="secondary" (click)="reset(); form.resetForm(model)" [disabled]="busy()">Təmizlə</button>
      </div>
    </form>
    <button type="button" class="secondary" (click)="load()" [disabled]="loading() || busy()">Siyahını yenilə</button>
    @if (loading()) { <p role="status">Yüklənir…</p> }
    <div class="table-wrap">
      <table>
        <thead><tr><th>Kod</th><th>Dərs</th><th>Sinif</th><th>Müəllim</th><th>Əməliyyat</th></tr></thead>
        <tbody>
          @for (lesson of lessons(); track lesson.code) {
            <tr>
              <td>{{ lesson.code }}</td><td>{{ lesson.name }}</td><td>{{ lesson.class }}</td>
              <td>{{ lesson.teacherFirstName }} {{ lesson.teacherLastName }}</td>
              <td><div class="actions">
                <button type="button" class="secondary" (click)="edit(lesson)" [disabled]="busy() || loading()">Redaktə</button>
                <button type="button" class="danger" (click)="remove(lesson)" [disabled]="busy() || loading()">Sil</button>
              </div></td>
            </tr>
          } @empty {
            @if (!loading()) { <tr><td colspan="5">Dərs qeyd olunmayıb.</td></tr> }
          }
        </tbody>
      </table>
    </div>
  `
})
export class LessonsComponent implements OnInit {
  private readonly api = inject(ApiService);
  readonly lessons = signal<Lesson[]>([]);
  readonly loading = signal(false);
  readonly busy = signal(false);
  readonly error = signal('');
  editing = false;
  model = this.empty();

  ngOnInit() { this.load(); }

  load() {
    this.loading.set(true);
    this.error.set('');
    this.api.getLessons().pipe(finalize(() => this.loading.set(false))).subscribe({
      next: lessons => this.lessons.set(lessons),
      error: error => this.error.set(this.api.errorMessage(error))
    });
  }

  edit(lesson: Lesson) {
    this.model = { ...lesson };
    this.editing = true;
    this.error.set('');
  }

  save() {
    if (this.busy() || this.loading()) return;
    this.busy.set(true);
    this.error.set('');
    const request: Observable<unknown> = this.editing ? this.api.updateLesson(this.model) : this.api.createLesson(this.model);
    request.pipe(finalize(() => this.busy.set(false))).subscribe({
      next: () => { this.reset(); this.load(); },
      error: error => this.error.set(this.api.errorMessage(error))
    });
  }

  remove(lesson: Lesson) {
    if (!window.confirm(`“${lesson.name}” dərsi silinsin?`)) return;
    this.busy.set(true);
    this.error.set('');
    this.api.deleteLesson(lesson.code).pipe(finalize(() => this.busy.set(false))).subscribe({
      next: () => {
        if (this.model.code === lesson.code) this.reset();
        this.load();
      },
      error: error => this.error.set(this.api.errorMessage(error))
    });
  }

  reset() { this.model = this.empty(); this.editing = false; }
  private empty(): Lesson {
    return { code: '', name: '', class: 1, teacherFirstName: '', teacherLastName: '' };
  }
}
