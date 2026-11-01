import { Injectable, Injector } from '@angular/core';
import { HttpErrorResponse, HttpEvent, HttpHandler, HttpInterceptor, HttpRequest } from '@angular/common/http';
import { Observable } from 'rxjs';
import { tap } from 'rxjs/operators';
import { ApiService } from './api.service';

// Sends the access token with every call to the API, and ends the session when the API says it is no longer valid.
@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  // ApiService is read when a request is made, because it depends on HttpClient and that depends on this interceptor.
  constructor(private injector: Injector) { }

  intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const service = this.injector.get(ApiService);
    const apiRoot = service.baseUrl + 'api/';

    if (request.url.indexOf(apiRoot) !== 0) {
      return next.handle(request);
    }

    // The sign-in call is the one that creates the token, so it does not carry one.
    const isSignIn = request.method === 'POST' && request.url === apiRoot + 'Login';
    const token = service.getToken();
    const authorized = token && !isSignIn
      ? request.clone({ setHeaders: { Authorization: 'Bearer ' + token } })
      : request;

    return next.handle(authorized).pipe(
      tap(() => { }, error => {
        if (error instanceof HttpErrorResponse && error.status === 401 && !isSignIn) {
          service.expireSession();
        }
      })
    );
  }
}
