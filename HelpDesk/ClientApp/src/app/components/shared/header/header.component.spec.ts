import { beforeEach, describe, expect, it, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { HeaderComponent } from './header.component';
import { ApiService } from '../../../services/api.service';
import { ShellState } from '../../../ui/shell-state.service';
import { ThemeService } from '../../../ui/theme.service';

describe('HeaderComponent', () => {
  let user: any;
  let theme: { choice: string, set: ReturnType<typeof vi.fn> };
  let shell: ShellState;

  function build() {
    const service: any = { getUser: () => user, hasImage: (i: any) => !!i, swal: vi.fn(() => Promise.resolve({ value: false })), closeSession: vi.fn() };
    theme = { choice: 'system', set: vi.fn((c: string) => { theme.choice = c; }) };
    shell = new ShellState();
    TestBed.configureTestingModule({
      providers: [provideRouter([]), { provide: ApiService, useValue: service }, { provide: ThemeService, useValue: theme }, { provide: ShellState, useValue: shell }]
    });
    const fixture = TestBed.createComponent(HeaderComponent);
    fixture.detectChanges();
    return fixture;
  }

  beforeEach(() => { user = { nombre: 'Ana Pérez', acceso: 'ADMINISTRADOR', empresa: 'Demo Company', image: null }; });

  it('shows the name, the role and the company of the person', () => {
    const el = build().nativeElement as HTMLElement;
    expect(el.querySelector('.hd-user-name')!.textContent).toBe('Ana Pérez');
    expect(el.querySelector('.hd-user-role')!.textContent).toBe('Administrador · Demo Company');
  });

  it('draws the initials when there is no photo', () => {
    const el = build().nativeElement as HTMLElement;
    expect(el.querySelector('.hd-avatar')!.textContent!.trim()).toBe('AP');
  });

  it('draws the photo when there is one', () => {
    user.image = 'abc';
    const el = build().nativeElement as HTMLElement;
    expect(el.querySelector('.hd-avatar img')!.getAttribute('src')).toBe('data:image/jpeg;base64,abc');
  });

  it('opens the menu from the avatar and tells screen readers', () => {
    const f = build(); const el = f.nativeElement as HTMLElement;
    expect(el.querySelector('.hd-user-menu')).toBeNull();
    (el.querySelector('.hd-avatar') as HTMLElement).click(); f.detectChanges();
    expect(el.querySelector('.hd-user-menu')).not.toBeNull();
    expect(el.querySelector('.hd-avatar')!.getAttribute('aria-expanded')).toBe('true');
  });

  it('closes the menu with Escape and with a click anywhere else', () => {
    const f = build(); const el = f.nativeElement as HTMLElement;
    (el.querySelector('.hd-avatar') as HTMLElement).click(); f.detectChanges();
    document.dispatchEvent(new KeyboardEvent('keydown', { key: 'Escape' })); f.detectChanges();
    expect(el.querySelector('.hd-user-menu')).toBeNull();
    (el.querySelector('.hd-avatar') as HTMLElement).click(); f.detectChanges();
    document.dispatchEvent(new MouseEvent('click')); f.detectChanges();
    expect(el.querySelector('.hd-user-menu')).toBeNull();
  });

  it('offers the three looks and marks the one in use', () => {
    const f = build(); const el = f.nativeElement as HTMLElement;
    (el.querySelector('.hd-avatar') as HTMLElement).click(); f.detectChanges();
    const buttons = [...el.querySelectorAll('.hd-theme-options button')] as HTMLElement[];
    expect(buttons.map(b => b.textContent!.trim())).toEqual(['Sistema', 'Claro', 'Oscuro']);
    expect(buttons.map(b => b.classList.contains('on'))).toEqual([true, false, false]);
    expect(buttons.map(b => b.getAttribute('aria-pressed'))).toEqual(['true', 'false', 'false']);
  });

  it('changes the look from the menu and keeps the menu open', () => {
    const f = build(); const el = f.nativeElement as HTMLElement;
    (el.querySelector('.hd-avatar') as HTMLElement).click(); f.detectChanges();
    ([...el.querySelectorAll('.hd-theme-options button')] as HTMLElement[])[2].click(); f.detectChanges();
    expect(theme.set).toHaveBeenCalledWith('dark');
    expect(el.querySelector('.hd-user-menu')).not.toBeNull();
    expect(([...el.querySelectorAll('.hd-theme-options button')] as HTMLElement[])[2].classList.contains('on')).toBe(true);
  });

  it('opens the side menu from the button of the narrow screens', () => {
    const f = build();
    (f.nativeElement.querySelector('.hd-menu-button') as HTMLElement).click();
    expect(shell.menuOpen).toBe(true);
  });
});

describe('HeaderComponent, pressing the buttons', () => {
  function open() {
    const service: any = { getUser: () => ({ nombre: 'Ana Pérez', acceso: 'ADMINISTRADOR', empresa: 'Demo', image: null }), hasImage: () => false, swal: vi.fn(() => Promise.resolve({ value: false })), closeSession: vi.fn() };
    const theme = { choice: 'system', set: vi.fn() };
    const shell = new ShellState();
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({ providers: [provideRouter([]), { provide: ApiService, useValue: service }, { provide: ThemeService, useValue: theme }, { provide: ShellState, useValue: shell }] });
    const fixture = TestBed.createComponent(HeaderComponent);
    fixture.detectChanges();
    return { fixture, el: fixture.nativeElement as HTMLElement, theme, shell };
  }

  it('the menu button opens and closes the side menu', () => {
    const { fixture, el, shell } = open();
    (el.querySelector('.hd-menu-button') as HTMLElement).click(); fixture.detectChanges();
    expect(shell.menuOpen).toBe(true);
    (el.querySelector('.hd-menu-button') as HTMLElement).click();
    expect(shell.menuOpen).toBe(false);
  });

  it('the three appearance buttons choose the theme', () => {
    const { fixture, el, theme } = open();
    (el.querySelector('.hd-avatar') as HTMLElement).click(); fixture.detectChanges();
    const buttons = Array.from(el.querySelectorAll<HTMLElement>('.hd-theme-options button'));
    buttons.forEach(b => b.click());
    expect(theme.set.mock.calls.map(c => c[0])).toEqual(['system', 'light', 'dark']);
  });

  it('"Mi perfil" closes the menu', () => {
    const { fixture, el } = open();
    (el.querySelector('.hd-avatar') as HTMLElement).click(); fixture.detectChanges();
    (el.querySelector('a[role="menuitem"]') as HTMLElement).click(); fixture.detectChanges();
    expect(el.querySelector('.hd-user-menu')).toBeNull();
  });
});
