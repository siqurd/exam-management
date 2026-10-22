import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Observable, finalize } from 'rxjs';
import { ApiService, Student } from './api.service';

@Component({
  selector: 'app-students',
  imports: [FormsModule],
  template: `
    <h2>Şagirdlər</h2>
    @if (error()) { <p class="error" role="alert">{{ error() }}</p> }
    <form #form="ngForm" (ngSubmit)="save()">
      <h3>{{ editing ? 'Şagirdi redaktə et' : 'Yeni şagird' }}</h3>
      <div class="fields">
        <label>Şagirdin nömrəsi
          <input type="number" name="number" [(ngModel)]="model.number" required min="1" max="99999" step="1" [readonly]="editing || busy()">
        </label>
        <label>Ad
          <input name="firstName" [(ngModel)]="model.firstName" required maxlength="30" [readonly]="busy()">
        </label>
        <label>Soyad
          <input name="lastName" [(ngModel)]="model.lastName" required maxlength="30" [readonly]="busy()">
        </label>
        <label>Sinif
          <input type="number" name="class" [(ngModel)]="model.class" required min="1" max="99" step="1" [readonly]="busy()">
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
        <thead><tr><th>Nömrə</th><th>Ad</th><th>Soyad</th><th>Sinif</th><th>Əməliyyat</th></tr></thead>
        <tbody>
          @for (student of students(); track student.number) {
            <tr>
              <td>{{ student.number }}</td><td>{{ student.firstName }}</td><td>{{ student.lastName }}</td><td>{{ student.class }}</td>
              <td><div class="actions">
                <button type="button" class="secondary" (click)="edit(student)" [disabled]="busy() || loading()">Redaktə</button>
                <button type="button" class="danger" (click)="remove(student)" [disabled]="busy() || loading()">Sil</button>
              </div></td>
            </tr>
          } @empty {
            @if (!loading()) { <tr><td colspan="5">Şagird qeyd olunmayıb.</td></tr> }
          }
        </tbody>
      </table>
    </div>
  `
})
export class StudentsComponent implements OnInit {
  private readonly api = inject(ApiService);
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
    this.api.getStudents().pipe(finalize(() => this.loading.set(false))).subscribe({
      next: students => this.students.set(students),
      error: error => this.error.set(this.api.errorMessage(error))
    });
  }

  edit(student: Student) {
    this.model = { ...student };
    this.editing = true;
    this.error.set('');
  }

  save() {
    if (this.busy() || this.loading()) return;
    this.busy.set(true);
    this.error.set('');
    const request: Observable<unknown> = this.editing ? this.api.updateStudent(this.model) : this.api.createStudent(this.model);
    request.pipe(finalize(() => this.busy.set(false))).subscribe({
      next: () => { this.reset(); this.load(); },
      error: error => this.error.set(this.api.errorMessage(error))
    });
  }

  remove(student: Student) {
    if (!window.confirm(`${student.firstName} ${student.lastName} silinsin?`)) return;
    this.busy.set(true);
    this.error.set('');
    this.api.deleteStudent(student.number).pipe(finalize(() => this.busy.set(false))).subscribe({
      next: () => {
        if (this.model.number === student.number) this.reset();
        this.load();
      },
      error: error => this.error.set(this.api.errorMessage(error))
    });
  }

  reset() { this.model = this.empty(); this.editing = false; }
  private empty(): Student { return { number: 1, firstName: '', lastName: '', class: 1 }; }
}
