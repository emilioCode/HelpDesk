import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { ThemeService } from './theme.service';

// A fake of the system setting that tests can flip, as the operating system does.
function fakeMedia(dark: boolean) {
  const listeners: Array<() => void> = [];
  const query: any = { matches: dark, addEventListener: (_: string, fn: () => void) => listeners.push(fn) };
  vi.stubGlobal('matchMedia', () => query);
  return { flip: (value: boolean) => { query.matches = value; listeners.forEach(fn => fn()); } };
}

describe('ThemeService', () => {
  beforeEach(() => {
    localStorage.clear();
    document.documentElement.removeAttribute('data-theme');
    document.head.insertAdjacentHTML('beforeend', '<meta name="theme-color" content="#2451D6">');
  });
  afterEach(() => {
    vi.unstubAllGlobals();
    document.querySelectorAll('meta[name="theme-color"]').forEach(m => m.remove());
  });

  it('follows the system when nothing was chosen', () => {
    fakeMedia(true);
    const theme = new ThemeService(); theme.init();
    expect(theme.choice).toBe('system');
    expect(document.documentElement.getAttribute('data-theme')).toBe('dark');
  });

  it('is light when the system is light', () => {
    fakeMedia(false);
    const theme = new ThemeService(); theme.init();
    expect(document.documentElement.getAttribute('data-theme')).toBe('light');
    expect(theme.dark).toBe(false);
  });

  it('a saved choice wins over the system', () => {
    fakeMedia(false);
    localStorage.setItem('hd-theme', 'dark');
    const theme = new ThemeService(); theme.init();
    expect(theme.choice).toBe('dark');
    expect(document.documentElement.getAttribute('data-theme')).toBe('dark');
  });

  it('keeps a choice for the next visit, and forgets it when it goes back to system', () => {
    fakeMedia(false);
    const theme = new ThemeService(); theme.init();
    theme.set('dark');
    expect(localStorage.getItem('hd-theme')).toBe('dark');
    expect(document.documentElement.getAttribute('data-theme')).toBe('dark');
    theme.set('system');
    expect(localStorage.getItem('hd-theme')).toBeNull();
    expect(document.documentElement.getAttribute('data-theme')).toBe('light');
  });

  it('ignores a saved value it does not know', () => {
    fakeMedia(false);
    localStorage.setItem('hd-theme', 'purple');
    expect(new ThemeService().choice).toBe('system');
  });

  it('changes with the system while the choice is "system", and only then', () => {
    const media = fakeMedia(false);
    const theme = new ThemeService(); theme.init();
    media.flip(true);
    expect(document.documentElement.getAttribute('data-theme')).toBe('dark');
    theme.set('light');
    media.flip(true);
    expect(document.documentElement.getAttribute('data-theme')).toBe('light');
  });

  it('paints the browser bar to match', () => {
    fakeMedia(false);
    const theme = new ThemeService(); theme.init();
    theme.set('dark');
    expect(document.querySelector('meta[name="theme-color"]')!.getAttribute('content')).toBe('#0E1218');
    theme.set('light');
    expect(document.querySelector('meta[name="theme-color"]')!.getAttribute('content')).toBe('#2451D6');
  });

  it('still works when the browser blocks its storage', () => {
    fakeMedia(false);
    vi.spyOn(Storage.prototype, 'setItem').mockImplementation(() => { throw new Error('blocked'); });
    vi.spyOn(Storage.prototype, 'getItem').mockImplementation(() => { throw new Error('blocked'); });
    const theme = new ThemeService(); theme.init();
    expect(() => theme.set('dark')).not.toThrow();
    expect(document.documentElement.getAttribute('data-theme')).toBe('dark');
    vi.restoreAllMocks();
  });
});
