import { Injectable } from '@angular/core';

export type ThemeChoice = 'system' | 'light' | 'dark';
const KEY = 'hd-theme';

// Light or dark look. "system" follows the device and changes with it. The choice is kept in this browser only.
// index.html applies the saved theme before the first paint, so there is no flash; this service keeps it in step afterwards.
@Injectable({ providedIn: 'root' })
export class ThemeService {
  choice: ThemeChoice = this.read();
  private query: MediaQueryList | null = null;

  init(): void {
    this.choice = this.read();
    this.query = typeof matchMedia === 'function' ? matchMedia('(prefers-color-scheme: dark)') : null;
    this.query?.addEventListener('change', () => { if (this.choice === 'system') this.apply(); });
    this.apply();
  }

  set(choice: ThemeChoice): void {
    this.choice = choice;
    try { if (choice === 'system') localStorage.removeItem(KEY); else localStorage.setItem(KEY, choice); } catch { /* private window or blocked storage: the choice lasts until reload */ }
    this.apply();
  }

  get dark(): boolean {
    return this.choice === 'dark' || (this.choice === 'system' && !!this.query?.matches);
  }

  private read(): ThemeChoice {
    try { const v = localStorage.getItem(KEY); return v === 'light' || v === 'dark' ? v : 'system'; } catch { return 'system'; }
  }

  private apply(): void {
    const root = document.documentElement;
    root.setAttribute('data-theme', this.dark ? 'dark' : 'light');
    document.querySelector('meta[name="theme-color"]')?.setAttribute('content', this.dark ? '#0E1218' : '#2451D6');
  }
}
