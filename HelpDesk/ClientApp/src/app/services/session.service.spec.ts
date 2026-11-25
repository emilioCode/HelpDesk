import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { SessionService } from './session.service';
import { fakeHub, FakeHub } from '../testing/screen-testing';

const HOUR = 3600_000;

describe('SessionService', () => {
  let hub: FakeHub;
  let user: any;
  let api: any;
  let session: SessionService;
  const zone: any = { runOutsideAngular: (fn: () => void) => fn(), run: (fn: () => any) => fn() };

  beforeEach(() => {
    vi.useFakeTimers();
    hub = fakeHub();
    user = { id: 2, idEmpresa: 1, token: 'tok', expiresAt: new Date(Date.now() + HOUR).toISOString(), tokenLifetimeMs: HOUR };
    api = {
      baseUrl: 'http://app/',
      messages: { titleSessionExpired: 'Sesión expirada', userDisabled: 'Deshabilitado', accessChanged: 'Cambió', titleSessionExpiring: 'Por expirar', sessionExpiring: 'Continuar?', continueSession: 'Continuar' },
      getUser: () => user,
      getToken: () => user && user.token,
      getTokenExpiry: () => user && user.expiresAt ? new Date(user.expiresAt) : null,
      expireSession: vi.fn(),
      closeSession: vi.fn(() => { user = null; }),
      swal: vi.fn(() => Promise.resolve({ value: true })),
      post: vi.fn(() => ({ subscribe: (ok: (r: any) => void) => ok({ token: 'new', expiresAt: new Date(Date.now() + HOUR).toISOString() }) })),
      setToken: vi.fn()
    };
    session = new SessionService(api, zone);
    vi.spyOn(console, 'error').mockImplementation(() => { });
  });
  afterEach(() => { vi.useRealTimers(); vi.restoreAllMocks(); });

  describe('the life of the token', () => {
    const expiresIn = (ms: number, lifetime = HOUR) => { user.expiresAt = new Date(Date.now() + ms).toISOString(); user.tokenLifetimeMs = lifetime; };

    it('does nothing when nobody is signed in', () => {
      user = null;
      session.check();
      expect(api.swal).not.toHaveBeenCalled();
      expect(api.post).not.toHaveBeenCalled();
    });

    it('ends the session when the token has expired', () => {
      expiresIn(-1000);
      session.check();
      expect(api.expireSession).toHaveBeenCalled();
    });

    it('leaves a token with most of its life alone', () => {
      session.check();
      expect(api.post).not.toHaveBeenCalled();
      expect(api.swal).not.toHaveBeenCalled();
    });

    it('asks for a new token when less than half is left and the person is active', () => {
      expiresIn(HOUR / 3);
      session.check();
      expect(api.post).toHaveBeenCalledWith('Login/Refresh', {});
      expect(api.setToken).toHaveBeenCalledWith('new', expect.any(String));
    });

    it('warns once when the person is idle and the token is about to end', () => {
      session.start();
      vi.advanceTimersByTime(HOUR - 5 * 60_000);       // the person did nothing for most of the hour
      user.expiresAt = new Date(Date.now() + 5000).toISOString();
      session.check(); session.check();
      expect(api.swal).toHaveBeenCalledTimes(1);
      expect(api.swal.mock.calls[0][0].title).toBe('Por expirar');
    });

    it('renews the token when the person chooses to continue', async () => {
      session.start();
      vi.advanceTimersByTime(HOUR - 5 * 60_000);
      user.expiresAt = new Date(Date.now() + 5000).toISOString();
      session.check();
      await vi.advanceTimersByTimeAsync(0);
      expect(api.post).toHaveBeenCalledWith('Login/Refresh', {});
    });

    it('does not ask for two tokens at once', () => {
      api.post = vi.fn(() => ({ subscribe: () => { } }));       // an answer that has not come yet
      expiresIn(HOUR / 3);
      session.check(); session.check();
      expect(api.post).toHaveBeenCalledTimes(1);
    });

    it('can ask again after a failed renewal', () => {
      api.post = vi.fn(() => ({ subscribe: (_: unknown, fail: () => void) => fail() }));
      expiresIn(HOUR / 3);
      session.check(); session.check();
      expect(api.post).toHaveBeenCalledTimes(2);
    });
  });

  describe('start', () => {
    it('checks the token every 15 seconds, and the notices every 2', () => {
      const check = vi.spyOn(session, 'check');
      session.start();
      vi.advanceTimersByTime(15_000);
      expect(check).toHaveBeenCalled();
    });

    it('counts clicks and keys as activity, so an active person is renewed', () => {
      session.start();
      vi.advanceTimersByTime(HOUR / 2 + 60_000);
      document.dispatchEvent(new Event('click'));
      user.expiresAt = new Date(Date.now() + HOUR / 3).toISOString();
      session.check();
      expect(api.post).toHaveBeenCalled();
    });

    it('checks again when the tab becomes visible', () => {
      const check = vi.spyOn(session, 'check');
      session.start();
      document.dispatchEvent(new Event('visibilitychange'));
      expect(check).toHaveBeenCalled();
    });
  });

  describe('notices of the server', () => {
    it('opens one connection while somebody is signed in', () => {
      session.start();
      expect(hub.start).toHaveBeenCalledTimes(1);
      session.check();
      expect(hub.start).toHaveBeenCalledTimes(1);
    });

    it('closes the connection after signing out', () => {
      session.start();
      user = null;
      session.check();
      expect(hub.stop).toHaveBeenCalled();
    });

    it('ends the session a second after the person was disabled', () => {
      session.start();
      hub.emit('refresh', 'session', 1, 2, 0);
      expect(api.closeSession).not.toHaveBeenCalled();
      vi.advanceTimersByTime(1000);
      expect(api.closeSession).toHaveBeenCalled();
      expect(api.swal).toHaveBeenCalledWith('Sesión expirada', 'Deshabilitado', 'info');
    });

    it('ends the session at once when the role or the company changed', () => {
      session.start();
      hub.emit('refresh', 'session', 1, 2, 2);
      expect(api.swal).toHaveBeenCalledWith('Sesión expirada', 'Cambió', 'info');
    });

    it('ignores the notices about other people and other kinds', () => {
      session.start();
      hub.emit('refresh', 'session', 1, 9, 0);
      hub.emit('refresh', 'ticket', 1, 2, 0);
      hub.emit('refresh', 'session', 2, 2, 0);
      vi.advanceTimersByTime(2000);
      expect(api.closeSession).not.toHaveBeenCalled();
    });

    it('survives a connection that does not start', async () => {
      hub.start.mockReturnValueOnce(Promise.reject(new Error('no')));
      session.start();
      await vi.advanceTimersByTimeAsync(0);
      expect(console.error).toHaveBeenCalled();
    });
  });
});
