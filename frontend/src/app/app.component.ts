import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  template: `
    <header>
      <h1>İmtahan qeydiyyatı</h1>
      <nav aria-label="Əsas menyu">
        <a routerLink="/lessons" routerLinkActive="active">Dərslər</a>
        <a routerLink="/students" routerLinkActive="active">Şagirdlər</a>
        <a routerLink="/exams" routerLinkActive="active">İmtahanlar</a>
      </nav>
    </header>
    <main><router-outlet /></main>
  `
})
export class AppComponent {}
