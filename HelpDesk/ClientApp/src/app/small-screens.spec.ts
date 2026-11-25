import { afterEach, describe, expect, it, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { ContentComponent } from './components/shared/content/content.component';
import { FooterComponent } from './components/shared/footer/footer.component';
import { CounterComponent } from './counter/counter.component';
import { FetchDataComponent } from './fetch-data/fetch-data.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { routes } from './app.routes';
import { appConfig, getBaseUrl } from './app.config';
import { openScreen, USERS } from './testing/screen-testing';

describe('the small screens and the setup', () => {
  afterEach(() => vi.restoreAllMocks());

  it('the content has a place for the screens', () => {
    const s = openScreen(ContentComponent, USERS.admin); s.detect();
    expect(s.element.querySelector('router-outlet')).not.toBeNull();
  });

  it('the footer names the company in the terms and conditions', () => {
    const s = openScreen(FooterComponent, USERS.admin); s.detect();
    expect(s.element.querySelector('#modal-copyright')).not.toBeNull();
    expect(s.element.textContent).toContain(s.service.company.name);
  });

  it('the counter counts', () => {
    const s = openScreen(CounterComponent, USERS.admin); s.detect();
    s.component.incrementCounter(); s.component.incrementCounter();
    expect(s.component.currentCount).toBe(2);
  });

  it('the sample forecast list keeps what the server sends, and logs a failure', () => {
    const s = openScreen(FetchDataComponent, USERS.admin);
    s.answer('api/SampleData/WeatherForecasts', [{ dateFormatted: 'x', temperatureC: 1, temperatureF: 34, summary: 'Cold' }]);
    s.detect();
    expect(s.component.forecasts.length).toBe(1);
    const f = openScreen(FetchDataComponent, USERS.admin);
    f.answer('api/SampleData/WeatherForecasts', {}, { status: 500 });
    expect(console.error).toHaveBeenCalled();
  });

  it('the old navigation bar opens and closes', () => {
    const s = openScreen(NavMenuComponent, USERS.admin); s.detect();
    s.component.toggle(); expect(s.component.isExpanded).toBe(true);
    s.component.collapse(); expect(s.component.isExpanded).toBe(false);
  });

  describe('the routes', () => {
    const guarded = routes.filter(r => r.component && r.path !== 't/:token');

    it('keep the public page of the customer free of the guard', () => {
      const tracking = routes.find(r => r.path === 't/:token')!;
      expect(tracking.canActivate).toBeUndefined();
    });

    it('guard every other screen and name its minimum role', () => {
      for (const r of guarded) {
        expect(r.canActivate, r.path).toBeDefined();
        expect(r.data && r.data['minLevel'], r.path).toBeGreaterThanOrEqual(1);
      }
    });

    it('keep the administration screens for administrators and the order lists for moderators', () => {
      const level = (path: string) => routes.find(r => r.path === path)!.data!['minLevel'];
      expect(level('user')).toBe(3);
      expect(level('business')).toBe(3);
      expect(level('report')).toBe(3);
      expect(level('ticket')).toBe(2);
      expect(level('inbox')).toBe(1);
    });

    it('give the order lists a title', () => {
      for (const p of ['ticket', 'inbox', 'waiting', 'finished']) expect(routes.find(r => r.path === p)!.data!['title'], p).toBeTruthy();
    });
  });

  describe('the setup', () => {
    it('reads the base address of the page', () => {
      document.head.insertAdjacentHTML('beforeend', '<base href="http://app/">');
      expect(getBaseUrl()).toBe('http://app/');
      document.head.querySelector('base')!.remove();
    });

    it('provides what the screens inject', () => {
      expect(appConfig.providers.length).toBeGreaterThan(5);
    });
  });
});
