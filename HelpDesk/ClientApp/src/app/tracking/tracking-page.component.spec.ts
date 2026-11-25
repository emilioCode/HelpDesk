import { beforeEach, describe, expect, it } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ActivatedRoute } from '@angular/router';
import { Meta, Title } from '@angular/platform-browser';
import { TrackingPageComponent } from './tracking-page.component';

const view = (over: any = {}) => ({
  orderNumber: 4, company: 'Demo Company', customer: 'Ferretería El Tornillo', serviceType: 'Servicio a Domicilio',
  status: 'inProgress', step: 2, progress: 50, updatedAt: '2026-09-29T00:00:00',
  companyPhone: '809-555-0100', companyEmail: 'soporte@demo.test', activities: [], ...over
});

describe('TrackingPageComponent', () => {
  let backend: HttpTestingController;

  function open(token = 'TOK/en') {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(), provideHttpClientTesting(),
        { provide: 'BASE_URL', useValue: 'http://app/' },
        { provide: ActivatedRoute, useValue: { snapshot: { paramMap: { get: () => token } } } }
      ]
    });
    backend = TestBed.inject(HttpTestingController);
    const fixture = TestBed.createComponent(TrackingPageComponent);
    fixture.detectChanges();
    return fixture;
  }

  beforeEach(() => { document.head.querySelectorAll('meta[name="robots"], meta[name="referrer"]').forEach(m => m.remove()); });

  it('asks the API for the order of the link, with the token encoded', () => {
    open('a/b c');
    const req = backend.expectOne('http://app/api/Tracking/a%2Fb%20c');
    expect(req.request.method).toBe('GET');
    req.flush(view());
  });

  it('shows a spinner while it waits', () => {
    const f = open();
    expect(f.componentInstance.state).toBe('loading');
    expect(f.nativeElement.querySelector('.hd-spin')).not.toBeNull();
    backend.expectOne(() => true).flush(view());
  });

  it('shows the order when the link is valid, and names the tab after it', () => {
    const f = open();
    backend.expectOne(() => true).flush(view());
    f.detectChanges();
    expect(f.componentInstance.state).toBe('ready');
    expect(f.nativeElement.textContent).toContain('Orden No. 4');
    expect(f.nativeElement.textContent).toContain('Ferretería El Tornillo');
    expect(TestBed.inject(Title).getTitle()).toBe('Orden No. 4 · Demo Company');
  });

  it('says the link is not valid when the API refuses it, without saying why', () => {
    const f = open();
    backend.expectOne(() => true).flush({}, { status: 404, statusText: 'Not Found' });
    f.detectChanges();
    expect(f.componentInstance.state).toBe('invalid');
    expect(f.nativeElement.textContent).toContain('Este enlace no es válido o ya venció');
    expect(TestBed.inject(Title).getTitle()).toBe('Enlace no válido');
  });

  it('asks search engines not to index the page and the browser not to send the address on', () => {
    open();
    backend.expectOne(() => true).flush(view());
    expect(TestBed.inject(Meta).getTag('name="robots"')!.content).toBe('noindex, nofollow');
    expect(TestBed.inject(Meta).getTag('name="referrer"')!.content).toBe('no-referrer');
  });

  describe('state of the order', () => {
    const open_ = (status: string, step: number) => { const f = open(); backend.expectOne(() => true).flush(view({ status, step })); f.detectChanges(); return f; };

    it('names each state in words and picks its tone', () => {
      const cases: Array<[string, string, string]> = [['open', 'Abierto', 'open'], ['inProgress', 'En proceso', 'prog'], ['completed', 'Completado', 'done'], ['approved', 'Aprobado', 'appr']];
      for (const [status, label, tone] of cases) {
        TestBed.resetTestingModule();
        const f = open_(status, 1);
        expect(f.componentInstance.statusLabel).toBe(label);
        expect(f.componentInstance.tone).toBe(tone);
      }
    });

    it('paints the progress green only when the order is approved', () => {
      expect(open_('approved', 4).componentInstance.fillTone).toBe('done');
      TestBed.resetTestingModule();
      expect(open_('completed', 3).componentInstance.fillTone).toBe('accent');
    });

    it('marks the steps behind, the one it is at, and the ones to come', () => {
      const c = open_('inProgress', 2).componentInstance;
      expect([0, 1, 2, 3].map(i => c.stepState(i))).toEqual(['done', 'current', 'pending', 'pending']);
    });

    it('marks every step done when the order is approved', () => {
      const c = open_('approved', 4).componentInstance;
      expect([0, 1, 2, 3].map(i => c.stepState(i))).toEqual(['done', 'done', 'done', 'done']);
    });
  });

  it('offers to call and to write only when the company has a phone and an e-mail', () => {
    const f = open();
    backend.expectOne(() => true).flush(view({ companyPhone: null, companyEmail: 'soporte@demo.test' }));
    f.detectChanges();
    const links = [...f.nativeElement.querySelectorAll('.tp-actions a')].map((a: any) => a.getAttribute('href'));
    expect(links).toEqual(['mailto:soporte@demo.test']);
  });
});
