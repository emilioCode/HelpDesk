import { beforeEach, describe, expect, it, vi } from 'vitest';
import { TestBed } from '@angular/core/testing';
import { HttpClient, provideHttpClient, withInterceptorsFromDi, HTTP_INTERCEPTORS } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { AuthInterceptor } from './auth.interceptor';
import { ApiService } from './api.service';

describe('AuthInterceptor', () => {
  let http: HttpClient;
  let backend: HttpTestingController;
  let fake: { baseUrl: string, getToken: () => string | null, expireSession: ReturnType<typeof vi.fn> };

  beforeEach(() => {
    fake = { baseUrl: 'http://app/', getToken: () => 'abc123', expireSession: vi.fn() };
    TestBed.configureTestingModule({
      providers: [
        { provide: ApiService, useValue: fake },
        { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true },
        provideHttpClient(withInterceptorsFromDi()),
        provideHttpClientTesting()
      ]
    });
    http = TestBed.inject(HttpClient);
    backend = TestBed.inject(HttpTestingController);
  });

  it('sends the token with every call to the API', () => {
    http.get('http://app/api/Ticket/1').subscribe();
    const req = backend.expectOne('http://app/api/Ticket/1');
    expect(req.request.headers.get('Authorization')).toBe('Bearer abc123');
    req.flush({});
  });

  it('does not send the token to anything that is not the API', () => {
    http.get('http://other/api/x').subscribe();
    const req = backend.expectOne('http://other/api/x');
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({});
  });

  it('does not send a token with the sign-in call, which creates it', () => {
    http.post('http://app/api/Login', {}).subscribe();
    const req = backend.expectOne('http://app/api/Login');
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({});
  });

  it('sends no header when nobody is signed in', () => {
    fake.getToken = () => null;
    http.get('http://app/api/Ticket').subscribe();
    const req = backend.expectOne('http://app/api/Ticket');
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush({});
  });

  it('ends the session when the API answers 401', () => {
    http.get('http://app/api/Ticket').subscribe({ error: () => { } });
    backend.expectOne('http://app/api/Ticket').flush({}, { status: 401, statusText: 'Unauthorized' });
    expect(fake.expireSession).toHaveBeenCalledTimes(1);
  });

  it('does not end the session when the sign-in itself answers 401 (a wrong password)', () => {
    http.post('http://app/api/Login', {}).subscribe({ error: () => { } });
    backend.expectOne('http://app/api/Login').flush({}, { status: 401, statusText: 'Unauthorized' });
    expect(fake.expireSession).not.toHaveBeenCalled();
  });

  it('leaves other errors to the screen', () => {
    http.get('http://app/api/Ticket').subscribe({ error: () => { } });
    backend.expectOne('http://app/api/Ticket').flush({}, { status: 500, statusText: 'Server Error' });
    expect(fake.expireSession).not.toHaveBeenCalled();
  });
});
