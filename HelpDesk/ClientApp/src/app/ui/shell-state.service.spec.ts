import { describe, expect, it } from 'vitest';
import { ShellState } from './shell-state.service';

describe('ShellState', () => {
  it('starts with the side menu closed', () => { expect(new ShellState().menuOpen).toBe(false); });
  it('opens and closes the menu with the button', () => {
    const s = new ShellState();
    s.toggle(); expect(s.menuOpen).toBe(true);
    s.toggle(); expect(s.menuOpen).toBe(false);
  });
  it('closes the menu when a link is chosen', () => {
    const s = new ShellState(); s.toggle(); s.close();
    expect(s.menuOpen).toBe(false);
  });
});
