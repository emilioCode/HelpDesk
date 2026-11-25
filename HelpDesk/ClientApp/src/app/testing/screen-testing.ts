import { ToastService } from '../ui/toast.service';
import { Provider, Type } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting, TestRequest } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import * as signalR from '@microsoft/signalr';
import { vi } from 'vitest';
import { ApiService } from '../services/api.service';
import { SessionStore } from '../services/session-store.service';

// What the tests of the screens share: a signed-in person, a fake of the server and of the real-time connection, and mute dialogs.

export const USERS = {
  admin: { id: 2, nombre: 'Ana Pérez', acceso: 'ADMINISTRADOR', idEmpresa: 1, empresa: 'Demo Company', token: 'tok', cuentaUsuario: 'admin', correo: 'ana@demo.test', image: null },
  root: { id: 1, nombre: 'Root', acceso: 'ROOT', idEmpresa: 1, empresa: 'Demo Company', token: 'tok', cuentaUsuario: 'root', correo: null, image: null },
  moderator: { id: 3, nombre: 'Luis Gómez', acceso: 'MODERADOR', idEmpresa: 1, empresa: 'Demo Company', token: 'tok', cuentaUsuario: 'mod', correo: 'luis@demo.test', image: null },
  technician: { id: 4, nombre: 'Carlos Ramírez', acceso: 'TECNICO', idEmpresa: 1, empresa: 'Demo Company', token: 'tok', cuentaUsuario: 'tecnico1', correo: 'carlos@demo.test', image: null }
};

export interface FakeHub {
  on: ReturnType<typeof vi.fn>;
  start: ReturnType<typeof vi.fn>;
  stop: ReturnType<typeof vi.fn>;
  invoke: ReturnType<typeof vi.fn>;
  handlers: Record<string, (...args: any[]) => void>;
  /** What the server would send: calls the handler the screen registered for the event. */
  emit: (event: string, ...args: any[]) => void;
}

/** Replaces the real-time connection that every screen opens with one the test can drive. */
export function fakeHub(): FakeHub {
  const handlers: Record<string, (...args: any[]) => void> = {};
  const hub: FakeHub = {
    handlers,
    on: vi.fn((event: string, fn: (...args: any[]) => void) => { handlers[event] = fn; }),
    start: vi.fn(() => Promise.resolve()),
    stop: vi.fn(() => Promise.resolve()),
    invoke: vi.fn(() => Promise.resolve()),
    emit: (event, ...args) => handlers[event]?.(...args)
  };
  vi.spyOn(signalR.HubConnectionBuilder.prototype, 'build').mockReturnValue(hub as any);
  return hub;
}

export interface Screen<T> {
  component: T;
  fixture: any;
  element: HTMLElement;
  service: ApiService;
  hub: FakeHub;
  http: HttpTestingController;
  swal: ReturnType<typeof vi.fn>;
  /** The toasts on show, as "title" or "title: message". */
  toasts: () => string[];
  /** Answers the next call whose URL ends with the path (after "api/"). */
  answer: (path: string | RegExp, body: unknown, options?: { status?: number }) => TestRequest;
  /** Answers every pending call with an empty list, so a screen that loads several things settles. */
  settle: (bodies?: Record<string, unknown>) => void;
  detect: () => void;
}

/** Builds a screen as the application does, signed in as `user`. The calls it makes while building wait for `answer` or `settle`. */
export function openScreen<T>(type: Type<T>, user: any = USERS.admin, extra: Provider[] = []): Screen<T> {
  TestBed.resetTestingModule();
  sessionStorage.clear();
  const hub = fakeHub();
  TestBed.configureTestingModule({
    providers: [
      provideHttpClient(), provideHttpClientTesting(), provideRouter([]),
      { provide: 'BASE_URL', useValue: 'http://app/' },
      ApiService, SessionStore,
      ...extra
    ]
  });
  if (user) new SessionStore().store('user', user);
  const swal = vi.fn(() => Promise.resolve({ value: true }));
  const service = TestBed.inject(ApiService);
  service.swal = swal as any;
  vi.spyOn(console, 'log').mockImplementation(() => { });
  vi.spyOn(console, 'error').mockImplementation(() => { });
  const http = TestBed.inject(HttpTestingController);
  const fixture = TestBed.createComponent(type);
  const screen: Screen<T> = {
    component: fixture.componentInstance, fixture, element: fixture.nativeElement, service, hub, http, swal,
    answer: (path, body, options) => {
      const req = http.expectOne(r => typeof path === 'string' ? r.url.endsWith(path) : path.test(r.url));
      req.flush(body as any, options?.status && options.status >= 400 ? { status: options.status, statusText: 'error' } : {});
      return req;
    },
    settle: (bodies = {}) => {
      http.match(() => true).forEach(r => {
        const key = Object.keys(bodies).find(k => r.request.url.endsWith(k));
        r.flush(key ? bodies[key] as any : []);
      });
    },
    toasts: () => TestBed.inject(ToastService).toasts().map(t => t.message ? t.title + ': ' + t.message : t.title),
    detect: () => fixture.detectChanges()
  };
  return screen;
}
