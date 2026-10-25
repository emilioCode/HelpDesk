import { Injectable, NgZone } from '@angular/core';
import { ApiService } from './api.service';

// Keeps the session alive while the user works, and warns them before it ends if they stop.
//  - When less than half of the token's life is left and the user has been active recently, it asks for a new token.
//  - When the user is idle and the token is about to expire, it asks whether to continue.
//  - When the token has expired the session is ended (the interceptor does the same if the API answers 401).
@Injectable()
export class SessionService {
  private lastActivity = Date.now();
  private warned = false;
  private refreshing = false;

  constructor(private service: ApiService, private zone: NgZone) { }

  start(): void {
    // Outside Angular's zone, so moving the mouse does not trigger change detection.
    this.zone.runOutsideAngular(() => {
      ['click', 'keydown', 'mousemove', 'touchstart', 'scroll'].forEach(name =>
        document.addEventListener(name, () => { this.lastActivity = Date.now(); }, true));
      document.addEventListener('visibilitychange', () => { if (!document.hidden) { this.check(); } });
      setInterval(() => this.check(), 15000);
    });
  }

  check(): void {
    const user = this.service.getUser();
    const expiry = this.service.getTokenExpiry();
    if (!user || !user.token || !expiry) {
      this.warned = false;
      return;
    }

    const remaining = expiry.getTime() - Date.now();
    if (remaining <= 0) {
      this.zone.run(() => this.service.expireSession());
      return;
    }

    const lifetime = user.tokenLifetimeMs || 3600000;
    const activeWindow = Math.min(10 * 60 * 1000, lifetime / 2);
    const idleFor = Date.now() - this.lastActivity;

    if (remaining < lifetime / 2 && idleFor < activeWindow) {
      this.refresh();
    } else if (remaining < Math.max(lifetime / 10, 20000) && !this.warned) {
      this.warned = true;
      this.zone.run(() => this.warn());
    }
  }

  private warn(): void {
    this.service.swal({
      title: this.service.messages.titleSessionExpiring,
      text: this.service.messages.sessionExpiring,
      type: 'warning',
      allowOutsideClick: false,
      confirmButtonText: this.service.messages.continueSession
    }).then(result => {
      if (result.value) {
        this.refresh();
      }
    });
  }

  refresh(): void {
    if (this.refreshing) {
      return;
    }
    this.refreshing = true;
    this.zone.run(() => {
      this.service.http.post(this.service.baseUrl + 'api/Login/Refresh', {}).subscribe(res => {
        this.service.setToken(res.token, res.expiresAt);
        this.warned = false;
        this.refreshing = false;
      }, () => {
        // A 401 means the session is over, and the interceptor has already ended it.
        this.refreshing = false;
      });
    });
  }
}
