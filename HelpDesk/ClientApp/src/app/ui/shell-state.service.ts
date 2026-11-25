import { Injectable } from '@angular/core';

// Whether the side menu is open on narrow screens (on wide screens it is always shown).
@Injectable({ providedIn: 'root' })
export class ShellState {
  menuOpen = false;
  toggle() { this.menuOpen = !this.menuOpen; }
  close() { this.menuOpen = false; }
}
