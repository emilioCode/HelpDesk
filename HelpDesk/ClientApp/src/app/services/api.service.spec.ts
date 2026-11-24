import { beforeEach, describe, expect, it, vi } from 'vitest';
import { of } from 'rxjs';
import { ApiService } from './api.service';
import { SessionStore } from './session-store.service';

describe('ApiService', () => {
  let service: ApiService;
  let store: SessionStore;
  let http: { get: any, post: any };
  let router: { navigateByUrl: any };

  beforeEach(() => {
    sessionStorage.clear();
    store = new SessionStore();
    http = { get: vi.fn(() => of({})), post: vi.fn(() => of({})) };
    router = { navigateByUrl: vi.fn() };
    service = new ApiService(store, router as any, 'http://app/', http as any);
    service.swal = vi.fn() as any;
  });

  describe('times', () => {
    it('writes the hour of the morning with AM', () => { expect(service.setTime('08:50:00')).toBe('8:50 AM'); });
    it('writes the afternoon with PM', () => { expect(service.setTime('13:30:00')).toBe('1:30 PM'); });
    it('writes midnight as 12 AM', () => { expect(service.setTime('00:05:00')).toBe('12:05 AM'); });
    it('writes noon as 12 PM', () => { expect(service.setTime('12:00:00')).toBe('12:00 PM'); });
    it('reads a time sent as an object of hours, minutes and seconds', () => {
      expect(service.setTime({ hours: 9, minutes: 5, seconds: 0 })).toBe('9:05 AM');
    });
    it('completes a time with zeros', () => {
      expect(service.ftmHour('8:5:0')).toBe('08:05:00');
      expect(service.ftmHour({ hours: 7, minutes: 0, seconds: 3 })).toBe('07:00:03');
    });
    it('does not print NaN for text that is not a time', () => {
      expect(service.ftmHour('')).toBe('00:00:00');
    });
    it('puts a zero before a single digit', () => {
      expect(service.fillZeroWithNumber(4)).toBe('04');
      expect(service.fillZeroWithNumber(12)).toBe('12');
    });
  });

  describe('values', () => {
    it('knows the level of each role', () => {
      expect(service.getLevel('ROOT')).toBe(4);
      expect(service.getLevel('ADMINISTRADOR')).toBe(3);
      expect(service.getLevel('MODERADOR')).toBe(2);
      expect(service.getLevel('TECNICO')).toBe(1);
    });
    it('counts a photo only when it has content', () => {
      expect(service.hasImage(null)).toBe(false);
      expect(service.hasImage('')).toBe(false);
      expect(service.hasImage('abc')).toBe(true);
    });
    it('tells an empty value from a filled one', () => {
      expect(service.isNullorEmpty('')).toBe(true);
      expect(service.isNullorEmpty(null)).toBe(true);
      expect(service.isNullorEmpty('x')).toBe(false);
      expect(service.validateTrim('   ')).toBe(true);
      expect(service.validateTrim(' a ')).toBe(false);
      expect(service.validate(undefined)).toBe(true);
    });
    it('replaces a character everywhere and quotes pasted text for the spreadsheet', () => {
      expect(service.replaceAll('a-b-c', '-', '/')).toBe('a/b/c');
      expect(service.setEspecialPasted('001')).toBe('="001"');
      expect(service.isNull(null)).toBe('');
      expect(service.isNull('x')).toBe('x');
    });
    it('lists the states and the types of request in the order of the screens', () => {
      expect(service.getStatuses().map(s => s.name)).toEqual(['Abierto', 'en Proceso', 'Completado']);
      expect(service.getTypeOfRequests().map(s => s.name)).toEqual(['Servicio Taller', 'Servicio a Domicilio']);
    });
    it('removes repeated items', () => {
      expect(service.getDistinct([1, 2, 2, 3, 1])).toEqual([1, 2, 3]);
    });
  });

  describe('calls', () => {
    it('adds "api/" to the path of every call', () => {
      service.get('Ticket/3/*');
      expect(http.get.mock.calls[0][0]).toBe('http://app/api/Ticket/3/*');
      service.post('Login', { a: 1 });
      expect(http.post.mock.calls[0][0]).toBe('http://app/api/Login');
      expect(http.post.mock.calls[0][1]).toEqual({ a: 1 });
    });
    it('sends null when a post has no body', () => {
      service.post('Login/Logout');
      expect(http.post.mock.calls[0][1]).toBeNull();
    });
  });

  describe('session', () => {
    it('has no token when nobody is signed in', () => {
      expect(service.getUser()).toBeNull();
      expect(service.getToken()).toBeNull();
      expect(service.getTokenExpiry()).toBeNull();
    });
    it('reads the token and its expiry from the stored user', () => {
      store.store('user', { id: 1, token: 'tok', expiresAt: '2030-01-01T00:00:00Z' });
      expect(service.getToken()).toBe('tok');
      expect(service.getTokenExpiry()!.toISOString()).toBe('2030-01-01T00:00:00.000Z');
    });
    it('stores a renewed token and how long it lasts', () => {
      store.store('user', { id: 1, token: 'old', expiresAt: '2000-01-01T00:00:00Z' });
      const later = new Date(Date.now() + 3600_000).toISOString();
      service.setToken('new', later);
      expect(service.getToken()).toBe('new');
      expect(service.getUser().tokenLifetimeMs).toBeGreaterThan(3_500_000);
    });
    it('ignores a renewed token when nobody is signed in', () => {
      service.setToken('new', '2030-01-01T00:00:00Z');
      expect(service.getUser()).toBeNull();
    });
    it('keeps the token when the profile data is refreshed', () => {
      store.store('user', { id: 2, token: 'tok', nombre: 'Ana' });
      service.updateSession([{ id: 2, nombre: 'Ana Pérez' }, { id: 3, nombre: 'Otro' }]);
      expect(service.getUser()).toMatchObject({ id: 2, token: 'tok', nombre: 'Ana Pérez' });
    });
    it('closing the session removes the user and asks the server to forget the cookie of the reports', () => {
      store.store('user', { id: 1, token: 'tok' });
      service.closeSession();
      expect(service.getUser()).toBeNull();
      expect(http.post.mock.calls[0][0]).toBe('http://app/api/Login/Logout');
      expect(router.navigateByUrl).toHaveBeenCalledWith('/');
    });
    it('ends the session once and says why', () => {
      store.store('user', { id: 1, token: 'tok' });
      service.expireSession();
      service.expireSession();
      expect(service.getUser()).toBeNull();
      expect((service.swal as any)).toHaveBeenCalledTimes(1);
      expect((service.swal as any).mock.calls[0][0]).toBe(service.messages.titleSessionExpired);
    });
    it('does nothing when nobody is signed in', () => {
      service.expireSession();
      expect((service.swal as any)).not.toHaveBeenCalled();
    });
  });

  describe('errors from the API', () => {
    beforeEach(() => { vi.spyOn(console, 'error').mockImplementation(() => { }); (service.swal as any).isVisible = () => false; service.isLoading = true; });

    it('shows the detail the server sent and stops the loading indicator', () => {
      service.handleHttpError({ status: 400, error: { errors: [{ detail: 'El RNC ya existe' }] } });
      expect(service.isLoading).toBe(false);
      expect(service.swal).toHaveBeenCalledWith('Warning', 'El RNC ya existe', 'warning');
    });
    it('joins the messages of a validation answer and uses its title', () => {
      service.handleHttpError({ status: 400, error: { title: 'Datos no válidos', errors: { nombre: ['Falta el nombre'], rnc: ['Falta el RNC'] } } });
      expect((service.swal as any).mock.calls[0][0]).toBe('Datos no válidos');
      expect((service.swal as any).mock.calls[0][1]).toContain('Falta el nombre');
    });
    it('says so when there is no connection', () => {
      service.handleHttpError({ status: 0 });
      expect(service.swal).toHaveBeenCalledWith(service.messages.titleError, service.messages.noConnection, 'error');
    });
    it('says so when the role is not allowed', () => {
      service.handleHttpError({ status: 403 });
      expect((service.swal as any).mock.calls[0][2]).toBe('warning');
    });
    it('says something for a server failure', () => {
      service.handleHttpError({ status: 500 });
      expect(service.swal).toHaveBeenCalledWith(service.messages.titleError, service.messages.unexpectedError, 'error');
    });
    it('leaves a 401 to the interceptor', () => {
      service.handleHttpError({ status: 401 });
      expect(service.swal).not.toHaveBeenCalled();
    });
    it('adds nothing over a popup that is already open', () => {
      (service.swal as any).isVisible = () => true;
      service.handleHttpError({ status: 500 });
      expect(service.swal).not.toHaveBeenCalled();
    });
    it('does not fail on an error without a body', () => {
      expect(() => service.handleHttpError(undefined)).not.toThrow();
    });
  });
});
