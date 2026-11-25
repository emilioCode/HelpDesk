import { Injectable, signal } from '@angular/core';

export interface Toast { id: number; title: string; message: string; }

// Short confirmations that do not stop the work ("saved", "link copied"). They fade by themselves.
// Anything the person must read or decide on (errors, warnings, questions) stays a window.
@Injectable({ providedIn: 'root' })
export class ToastService {
  readonly toasts = signal<Toast[]>([]);
  private next = 1;

  show(title: string, message = '', milliseconds = 4500): void {
    const id = this.next++;
    this.toasts.update(list => [...list.slice(-2), { id, title, message }]);
    if (milliseconds > 0) { setTimeout(() => this.dismiss(id), milliseconds); }
  }

  dismiss(id: number): void {
    this.toasts.update(list => list.filter(t => t.id !== id));
  }
}
