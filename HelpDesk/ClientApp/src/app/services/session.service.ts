import { Injectable, NgZone } from '@angular/core';
import * as signalR from '@microsoft/signalr';
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
  private notices: signalR.HubConnection;

  constructor(private service: ApiService, private zone: NgZone) { }

  start(): void {
    // Outside Angular's zone, so moving the mouse does not trigger change detection.
    this.zone.runOutsideAngular(() => {
      ['click', 'keydown', 'mousemove', 'touchstart', 'scroll'].forEach(name =>
        document.addEventListener(name, () => { this.lastActivity = Date.now(); }, true));
      document.addEventListener('visibilitychange', () => { if (!document.hidden) { this.check(); } });
      setInterval(() => this.check(), 15000);
      // Cheap, so the notice connection closes right after signing out instead of up to 15 seconds later.
      setInterval(() => this.syncNotices(), 2000);
    });
    this.syncNotices();
  }

  // One connection for the whole session, whatever screen is open, to receive the notice that the user was disabled.
  // It exists only while someone is signed in.
  private syncNotices(): void {
    const signedIn = !!this.service.getToken();
    if (signedIn && !this.notices) {
      this.notices = new signalR.HubConnectionBuilder()
        .withUrl(this.service.baseUrl + 'hub', { accessTokenFactory: () => this.service.getToken() })
        .build();
      this.notices.on('refresh', (component, idEmpresa, idUsuario, idOther) => {
        const user = this.service.getUser();
        if (component === 'session' && user && idEmpresa == user.idEmpresa && idUsuario == user.id && idOther == 0) {
          // Screens that handle this notice themselves act first; this covers the ones that do not.
          setTimeout(() => this.zone.run(() => this.endDisabledSession()), 1000);
        }
      });
      this.notices.start().catch(err => console.error(err.toString()));
    } else if (!signedIn && this.notices) {
      this.notices.stop().catch(() => {});
      this.notices = null;
    }
  }

  private endDisabledSession(): void {
    if (!this.service.getUser()) {
      return;
    }
    this.service.closeSession();
    this.service.swal(this.service.messages.titleSessionExpired, this.service.messages.userDisabled, 'info');
  }

  check(): void {
    this.syncNotices();
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
