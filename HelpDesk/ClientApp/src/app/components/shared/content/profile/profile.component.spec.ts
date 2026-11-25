import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { ProfileComponent } from './profile.component';
import { openScreen, USERS } from '../../../../testing/screen-testing';

const me = (over: any = {}) => ({ id: 2, nombre: 'Ana Pérez', cuentaUsuario: 'admin', correo: 'ana@demo.test', idEmpresa: 1, acceso: 'ADMINISTRADOR', habilitado: true, contrasena: 'x', ...over });

function open(user: any = USERS.admin) {
  const s = openScreen(ProfileComponent, user);
  s.answer('User/' + user.id + '/UNIQUE', [me({ id: user.id, acceso: user.acceso })]);
  s.answer('Business/' + user.id, [{ id: 1, razonSocial: 'Demo Company', image: null }]);
  s.answer('Ticket/numbersOfTickets/' + user.id, { data: { enCurso: 3, pendientes: 2, cerrados: 7 } });
  s.detect();
  return s;
}

describe('ProfileComponent', () => {
  const alertSpy = vi.fn();
  beforeEach(() => { alertSpy.mockClear(); vi.stubGlobal('alert', alertSpy); });
  afterEach(() => { vi.restoreAllMocks(); vi.unstubAllGlobals(); });

  it('shows the data of the person and the numbers of their orders', () => {
    const s = open();
    expect(s.component.form.get('cuentaUsuario')!.value).toBe('admin');
    expect(s.component.form.get('contrasena')!.value).toBeFalsy();
    expect(s.component.numbers).toEqual({ enCurso: 3, pendientes: 2, cerrados: 7 });
    expect(s.element.textContent).toContain('Ana Pérez');
  });

  it('does not let a person who is not root change the company', () => {
    expect(open().component.form.get('idEmpresa')!.disabled).toBe(true);
    expect(open(USERS.root).component.form.get('idEmpresa')!.disabled).toBe(false);
  });

  it('shows the error when the profile cannot be loaded', () => {
    const s = openScreen(ProfileComponent);
    s.detect();
    s.answer('User/2/UNIQUE', { errors: [{ detail: 'Caído' }] }, { status: 400 });
    s.settle({ 'Business/2': [{ id: 1, image: null }] });
    expect(s.swal).toHaveBeenCalledWith('Warning', 'Caído', 'warning');
  });

  describe('the new password', () => {
    it('asks for 6 characters and for the same text twice', () => {
      const s = open();
      const f = s.component.form;
      f.patchValue({ contrasena: '123', confirmacion: '123' });
      expect(f.get('contrasena')!.hasError('minlength')).toBe(true);
      f.patchValue({ contrasena: '123456', confirmacion: '654321' });
      expect(f.hasError('mismatch')).toBe(true);
      f.patchValue({ confirmacion: '123456' });
      expect(f.valid).toBe(true);
    });

    it('does not send an invalid form and says what is missing', () => {
      const s = open();
      s.component.edit();
      expect(s.swal).toHaveBeenCalledWith(s.service.messages.titleRequiredFields, expect.any(String), 'info');
      s.http.expectNone(r => r.method === 'POST');
    });

    it('sends the new password and reloads the profile', () => {
      const s = open();
      s.component.form.patchValue({ contrasena: '123456', confirmacion: '123456' });
      s.component.edit();
      const req = s.http.expectOne(r => r.url.endsWith('User/Put/2'));
      expect(req.request.body).toMatchObject({ id: 2, contrasena: '123456' });
      req.flush({ code: '1', title: 'Listo', message: 'ok', icon: 'success' });
      s.settle({ 'User/2/UNIQUE': [me()], 'Business/2': [{ id: 1, image: null }] });
      expect(s.swal).toHaveBeenCalledWith('Listo', 'ok', 'success');
    });

    it('ends the session when root changed the company of the person', () => {
      const s = open(USERS.root);
      s.component.form.patchValue({ contrasena: '123456', confirmacion: '123456', idEmpresa: 5 });
      s.component.edit();
      s.http.expectOne(r => r.url.endsWith('User/Put/1')).flush({ code: '1', title: 'Listo', message: 'ok', icon: 'success' });
      expect(s.swal).toHaveBeenCalledWith(s.service.messages.titleSessionExpired, s.service.messages.accessChanged, 'info');
      s.http.expectOne(r => r.url.endsWith('Login/Logout'));
    });

    it('shows the error when saving fails', () => {
      const s = open();
      s.component.form.patchValue({ contrasena: '123456', confirmacion: '123456' });
      s.component.edit();
      s.http.expectOne(r => r.url.endsWith('User/Put/2')).flush({ errors: [{ detail: 'No' }] }, { status: 400, statusText: 'Bad Request' });
      expect(s.swal).toHaveBeenCalledWith('Warning', 'No', 'warning');
      expect(s.service.isLoading).toBe(false);
    });
  });

  describe('real-time notices', () => {
    it('reloads the profile when it changes and keeps what was typed', () => {
      const s = open();
      s.component.form.patchValue({ contrasena: '123456', confirmacion: '123456' });
      s.hub.emit('refresh', 'users', 1, 2, 0);
      s.answer('User/2/UNIQUE', [me({ nombre: 'Ana P.' })]);
      expect(s.component.userT.nombre).toBe('Ana P.');
      expect(s.component.form.get('contrasena')!.value).toBe('123456');
    });

    it('shows the error when the reload fails', () => {
      const s = open();
      s.hub.emit('refresh', 'users', 1, 2, 0);
      s.answer('User/2/UNIQUE', { errors: [{ detail: 'Caído' }] }, { status: 400 });
      expect(s.swal).toHaveBeenCalled();
    });

    it('ends the session when the person was disabled', () => {
      const s = open();
      s.hub.emit('refresh', 'session', 1, 2, 0);
      expect(alertSpy).toHaveBeenCalledWith(s.service.messages.userDisabled);
      s.http.expectOne(r => r.url.endsWith('Login/Logout'));
    });

    it('ignores the notices about other people', () => {
      const s = open();
      s.hub.emit('refresh', 'users', 1, 9, 0);
      s.hub.emit('refresh', 'session', 1, 9, 0);
      s.http.expectNone(() => true);
    });
  });

  it('shows the error when the numbers cannot be loaded', () => {
    const s = openScreen(ProfileComponent);
    s.detect();
    s.answer('User/2/UNIQUE', [me()]);
    s.answer('Business/2', [{ id: 1, image: null }]);
    s.answer('Ticket/numbersOfTickets/2', { errors: [{ detail: 'Caído' }] }, { status: 400 });
    expect(s.swal).toHaveBeenCalled();
  });

  it('closes the real-time connection with the screen', () => {
    const s = open();
    s.fixture.destroy();
    expect(s.hub.stop).toHaveBeenCalled();
  });
});
