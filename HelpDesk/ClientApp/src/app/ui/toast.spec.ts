import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { HdToastsComponent } from './hd-toasts.component';
import { ToastService } from './toast.service';
import { ApiService } from '../services/api.service';
import { SessionStore } from '../services/session-store.service';
import { provideHttpClient } from '@angular/common/http';
import { provideRouter } from '@angular/router';

describe('toasts', () => {
  beforeEach(() => vi.useFakeTimers());
  afterEach(() => vi.useRealTimers());

  it('show a message and fade by themselves', () => {
    const toast = TestBed.inject(ToastService);
    toast.show('Listo', 'Guardado');
    expect(toast.toasts().length).toBe(1);
    vi.advanceTimersByTime(5000);
    expect(toast.toasts().length).toBe(0);
  });

  it('keep the last three at most', () => {
    const toast = TestBed.inject(ToastService);
    ['a', 'b', 'c', 'd'].forEach(t => toast.show(t));
    expect(toast.toasts().map(t => t.title)).toEqual(['b', 'c', 'd']);
  });

  it('are drawn in a polite live region and can be closed', () => {
    const fixture = TestBed.createComponent(HdToastsComponent);
    TestBed.inject(ToastService).show('Listo', 'Guardado');
    fixture.detectChanges();
    const el: HTMLElement = fixture.nativeElement;
    expect(el.querySelector('[role="status"][aria-live="polite"]')).not.toBeNull();
    expect(el.textContent).toContain('Guardado');
    (el.querySelector('.hd-toast-close') as HTMLButtonElement).click();
    fixture.detectChanges();
    expect(el.querySelector('.hd-toast')).toBeNull();
  });

  it('are used by notify for a success, while anything else stays a window', () => {
    TestBed.configureTestingModule({ providers: [provideHttpClient(), provideRouter([]), SessionStore, ApiService, { provide: 'BASE_URL', useValue: 'http://app/' }] });
    const service = TestBed.inject(ApiService);
    const swal = vi.fn();
    service.swal = swal as any;
    service.notify('Listo', 'ok', 'success');
    expect(swal).not.toHaveBeenCalled();
    expect(TestBed.inject(ToastService).toasts().length).toBe(1);
    service.notify('No', 'Falló', 'warning');
    expect(swal).toHaveBeenCalledWith('No', 'Falló', 'warning');
  });
});
