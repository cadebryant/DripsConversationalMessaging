import { Component, effect, signal } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  standalone: false,
  styleUrl: './app.component.css'
})
export class AppComponent {
  protected readonly isDark = signal(localStorage.getItem('theme') !== 'light');

  constructor() {
    // Apply synchronously so the initial render already has the correct theme.
    // effect() alone is async and fires after Angular's first render pass,
    // which leaves the body in light mode until change detection runs.
    document.documentElement.setAttribute('data-theme', this.isDark() ? 'dark' : 'light');

    effect(() => {
      document.documentElement.setAttribute('data-theme', this.isDark() ? 'dark' : 'light');
      localStorage.setItem('theme', this.isDark() ? 'dark' : 'light');
    });
  }

  protected toggleTheme(): void {
    this.isDark.update(v => !v);
  }
}
