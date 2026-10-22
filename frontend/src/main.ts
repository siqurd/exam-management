import { bootstrapApplication } from '@angular/platform-browser';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';
import { AppComponent } from './app/app.component';
import { LessonsComponent } from './app/lessons.component';
import { StudentsComponent } from './app/students.component';
import { ExamsComponent } from './app/exams.component';

bootstrapApplication(AppComponent, {
  providers: [
    provideHttpClient(),
    provideRouter([
      { path: '', redirectTo: 'lessons', pathMatch: 'full' },
      { path: 'lessons', component: LessonsComponent },
      { path: 'students', component: StudentsComponent },
      { path: 'exams', component: ExamsComponent },
      { path: '**', redirectTo: 'lessons' }
    ])
  ]
}).catch(console.error);
