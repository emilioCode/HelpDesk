import { afterEach, describe, expect, it, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { AppComponent } from './app.component';
import { SessionService } from './services/session.service';
import { openScreen, USERS } from './testing/screen-testing';

const open = (user: any) => {
  const start = vi.fn();
  const s = openScreen(AppComponent, user, [{ provide: SessionService, useValue: { start } }]);
  s.detect();
  return { s, start };
};

describe('AppComponent', () => {
  afterEach(() => vi.restoreAllMocks());

  it('shows the sign in when nobody is signed in', () => {
    const { s } = open(null);
    expect(s.element.querySelector('app-login')).not.toBeNull();
    expect(s.element.querySelector('app-menu')).toBeNull();
  });

  it('shows the menu, the header, the content and the footer once signed in', () => {
    const { s } = open(USERS.technician);
    ['app-menu', 'app-header', 'app-content', 'app-footer'].forEach(tag => expect(s.element.querySelector(tag), tag).not.toBeNull());
    expect(s.element.querySelector('app-login')).toBeNull();
    s.settle();
  });

  it('starts to take care of the session', () => {
    const { s, start } = open(null);
    expect(start).toHaveBeenCalledTimes(1);
    expect(s.component.title).toBe('HelpDesk');
  });

  it('puts the default photo when a picture fails to load, once', () => {
    const { s } = open(null);
    const img = document.createElement('img');
    document.body.appendChild(img);
    img.dispatchEvent(new Event('error'));
    expect(img.src).toContain('assets/dist/img/photo_default.png');
    img.src = 'http://app/other.png';
    img.dispatchEvent(new Event('error'));
    expect(img.src).toBe('http://app/other.png');
    img.remove();
    expect(s.component).toBeTruthy();
  });

  it('does not care for the session on the public page of a customer', () => {
    const start = vi.fn();
    history.pushState({}, '', '/t/abc');
    TestBed.resetTestingModule();
    const s = openScreen(AppComponent, null, [{ provide: SessionService, useValue: { start } }]);
    s.detect();
    expect(s.component.publicPage).toBe(true);
    expect(start).not.toHaveBeenCalled();
    expect(s.element.querySelector('app-login')).toBeNull();
    history.pushState({}, '', '/');
  });
});
