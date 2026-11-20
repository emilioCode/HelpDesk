import { ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptorsFromDi, withXhr } from '@angular/common/http';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { ApiService } from './services/api.service';
import { AuthGuard } from './services/auth.guard';
import { AuthInterceptor } from './services/auth.interceptor';
import { SessionService } from './services/session.service';
import { SessionStore } from './services/session-store.service';

export function getBaseUrl() {
  return document.getElementsByTagName('base')[0].href;
}

export const appConfig: ApplicationConfig = {
  providers: [
    // The screens change objects by hand and the forms are synchronised in ngDoCheck, so change detection stays driven by zone.js.
    provideZoneChangeDetection(),
    { provide: 'BASE_URL', useFactory: getBaseUrl, deps: [] },
    ApiService,
    AuthGuard,
    SessionService,
    SessionStore,
    { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true },
    provideHttpClient(withXhr(), withInterceptorsFromDi()),
    provideRouter(routes)
  ]
};
