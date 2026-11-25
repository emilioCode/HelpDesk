import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { UserComponent } from './user.component';
import { openScreen, USERS } from '../../../../testing/screen-testing';

const users = [
  { id: 2, nombre: 'Ana Pérez', numDocumento: '001-1', cuentaUsuario: 'admin', correo: 'ana@demo.test', acceso: 'ADMINISTRADOR', idEmpresa: 1, habilitado: true, image: null },
  { id: 4, nombre: 'Carlos Ramírez', numDocumento: '001-3', cuentaUsuario: 'tecnico1', correo: 'carlos@demo.test', acceso: 'TECNICO', idEmpresa: 1, habilitado: true, image: null }
];

function open(user: any = USERS.admin) {
  const s = openScreen(UserComponent, user);
  if (user.acceso === 'ADMINISTRADOR' || user.acceso === 'ROOT') s.answer('User/' + user.id + '/*', users);
  s.detect();
  return s;
}

describe('UserComponent', () => {
  const alertSpy = vi.fn();
  beforeEach(() => { alertSpy.mockClear(); vi.stubGlobal('alert', alertSpy); });
  afterEach(() => { vi.restoreAllMocks(); vi.unstubAllGlobals(); });

  it('lists the users of the company', () => {
    const s = open();
    expect(s.element.querySelectorAll('tbody tr').length).toBe(2);
    expect(s.element.textContent).toContain('Carlos Ramírez');
  });

  it('keeps out a person below administrator, with a warning and a way home', () => {
    const s = openScreen(UserComponent, USERS.technician);
    s.detect();
    expect(alertSpy).toHaveBeenCalledWith(s.service.messages.noPermissionToAccess);
    s.http.expectNone(r => r.url.includes('/User/'));
  });

  it('shows the error when the list cannot be loaded', () => {
    const s = openScreen(UserComponent);
    s.detect();
    s.answer('User/2/*', { errors: [{ detail: 'Caído' }] }, { status: 400 });
    expect(s.swal).toHaveBeenCalledWith('Warning', 'Caído', 'warning');
  });

  describe('the window of a user', () => {
    it('asks for a password of 6 characters or more when adding', () => {
      const s = open();
      s.component.fillModal('add', {}); s.answer('Business/2', []);
      const f = s.component.form;
      f.patchValue({ nombre: 'N', numDocumento: '1', cuentaUsuario: 'u1', idEmpresa: 1, acceso: 'TECNICO', contrasena: '123' });
      expect(f.valid).toBe(false);
      f.patchValue({ contrasena: '123456' });
      expect(f.valid).toBe(true);
    });

    it('does not ask for a password when editing, but if there is one it needs 6 characters', () => {
      const s = open();
      s.component.fillModal('edit', { ...users[1] }); s.answer('Business/2', []);
      expect(s.component.form.valid).toBe(true);
      s.component.form.patchValue({ contrasena: '12' });
      expect(s.component.form.get('contrasena')!.hasError('minlength')).toBe(true);
    });

    it('locks the account name when editing, except for root', () => {
      const a = open();
      a.component.fillModal('edit', { ...users[1] }); a.answer('Business/2', []);
      expect(a.component.form.get('cuentaUsuario')!.disabled).toBe(true);
      const r = open(USERS.root);
      r.component.fillModal('edit', { ...users[1] }); r.answer('Business/1', []);
      expect(r.component.form.get('cuentaUsuario')!.disabled).toBe(false);
    });

    it('offers only the roles up to the one of the person', () => {
      const s = open();
      s.component.fillModal('add', {}); s.answer('Business/2', []);
      expect(s.component.levels.map((l: any) => l.name)).toEqual(['ADMINISTRADOR', 'MODERADOR', 'TECNICO']);
    });

    it('disables the whole form when the window asks to disable', () => {
      const s = open();
      s.component.fillModal('delete', { ...users[1] }); s.answer('Business/2', []);
      expect(s.component.form.disabled).toBe(true);
    });

    it('checks the e-mail', () => {
      const s = open();
      s.component.fillModal('add', {}); s.answer('Business/2', []);
      s.component.form.patchValue({ correo: 'no es correo' });
      expect(s.component.form.get('correo')!.hasError('email')).toBe(true);
    });
  });

  describe('the photo', () => {
    it('keeps the picture that was chosen, as text of base64', async () => {
      const s = open();
      s.component.fillModal('edit', { ...users[1] }); s.answer('Business/2', []);
      const file = new File(['abc'], 'a.png', { type: 'image/png' });
      s.component.handleFileInput({ item: () => file } as any);
      await vi.waitFor(() => expect(s.component.user.image).toBe('YWJj'));
      expect(s.component.imageUrl).toContain('data:image/png;base64,YWJj');
    });
  });

  describe('saving', () => {
    const ready = (option: 'add' | 'edit' | 'delete', who: any = users[1]) => {
      const s = open();
      s.component.fillModal(option, option === 'add' ? {} : { ...who }); s.answer('Business/2', []);
      s.detect();
      return s;
    };

    it('adds a user, tells the others and closes the window', () => {
      const s = ready('add');
      s.component.form.patchValue({ nombre: 'Nuevo', numDocumento: '9', cuentaUsuario: 'n1', acceso: 'TECNICO', contrasena: '123456' });
      const closed = vi.fn(); s.element.querySelector('#closeModalBtn')!.addEventListener('click', closed);
      s.component.add();
      const req = s.http.expectOne(r => r.url.endsWith('api/User') && r.method === 'POST');
      expect(req.request.body).toMatchObject({ nombre: 'Nuevo', cuentaUsuario: 'n1', contrasena: '123456', idEmpresa: 1 });
      req.flush({ code: '1', title: 'Listo', message: 'ok', icon: 'success' });
      expect(s.hub.invoke).toHaveBeenCalledWith('refresh', 'users', 1, undefined, 0);
      expect(closed).toHaveBeenCalled();
    });

    it('explains why adding failed', () => {
      const s = ready('add');
      s.component.add();
      s.http.expectOne(r => r.method === 'POST').flush({ title: 'Datos no válidos', errors: { cuentaUsuario: ['Ya existe'] } }, { status: 400, statusText: 'Bad Request' });
      expect(s.swal).toHaveBeenCalledWith('Datos no válidos', 'Ya existe', 'warning');
      expect(s.service.isLoading).toBe(false);
    });

    it('edits a user with the id of the person who edits in the address', () => {
      const s = ready('edit');
      s.component.form.patchValue({ nombre: 'Carlos R.' });
      s.component.edit();
      const req = s.http.expectOne(r => r.url.endsWith('User/Put/2'));
      expect(req.request.body).toMatchObject({ id: 4, nombre: 'Carlos R.', habilitado: true });
      req.flush({ code: '1', title: 'Listo', message: 'ok', icon: 'success' });
      expect(s.hub.invoke).toHaveBeenCalledWith('refresh', 'users', 1, 4, 0);
    });

    it('says the request failed when the connection is lost while editing', () => {
      const s = ready('edit');
      s.component.edit();
      s.http.expectOne(r => r.url.endsWith('User/Put/2')).error(new ProgressEvent('error'));
      expect((s.swal.mock.calls[0] as any[])[1]).toContain('could not be completed');
    });

    it('disables a user and tells the session of that person', () => {
      const s = ready('delete');
      s.component.delete();
      const req = s.http.expectOne(r => r.url.endsWith('User/Put/4'));
      expect(req.request.body.habilitado).toBe(false);
      req.flush({ code: '1', title: 'Listo', message: 'ok', icon: 'success' });
      expect(s.hub.invoke).toHaveBeenCalledWith('refresh', 'users', 1, 4, 0);
      expect(s.hub.invoke).toHaveBeenCalledWith('refresh', 'session', 1, 4, 0);
    });

    it('enables a user that was disabled and says so to its session', () => {
      const s = ready('delete', { ...users[1], habilitado: false });
      s.component.delete();
      s.http.expectOne(r => r.url.endsWith('User/Put/4')).flush({ code: '1', title: 'Listo', message: 'ok', icon: 'success' });
      expect(s.hub.invoke).toHaveBeenCalledWith('refresh', 'session', 1, 4, 1);
    });

    it('shows the error when disabling fails', () => {
      const s = ready('delete');
      s.component.delete();
      s.http.expectOne(r => r.url.endsWith('User/Put/4')).flush({ errors: [{ detail: 'No' }] }, { status: 400, statusText: 'Bad Request' });
      expect(s.swal).toHaveBeenCalledWith('Warning', 'No', 'warning');
    });
  });

  describe('the account name', () => {
    const typed = (value: string | null, answer?: unknown, status = 200) => {
      const s = open();
      s.component.fillModal('add', {}); s.answer('Business/2', []);
      s.component.validateUserAccount(value);
      if (answer !== undefined) s.answer('Login/1/' + value, answer, { status });
      return s;
    };

    it('says it is free', () => {
      const s = typed('nuevo', true);
      expect(s.component.renderHTML1).toBe('has-success has-feedback');
      expect(s.component.renderHTML2).toContain('fa-check');
      expect(s.component.message).toBe("It's ok.");
    });

    it('warns that another account has that name', () => {
      const s = typed('admin', false);
      expect(s.component.renderHTML1).toBe('has-warning has-feedback');
      expect(s.component.renderHTML2).toContain('fa-exclamation-triangle');
      expect(s.component.message).toContain('another account');
    });

    it('says nothing for an empty name, and asks nothing', () => {
      const s = typed('');
      expect(s.component.message).toBe('');
      s.http.expectNone(r => r.url.includes('Login/'));
      const n = typed(null);
      expect(n.component.renderHTML1).toBe('');
    });

    it('shows the error when the check fails', () => {
      const s = typed('x', { errors: [{ detail: 'Caído' }] }, 400);
      expect(s.swal).toHaveBeenCalledWith('Warning', 'Caído', 'warning');
      expect(s.service.isLoading).toBe(false);
    });
  });

  describe('real-time notices', () => {
    it('reloads the list and refreshes the session of the person', () => {
      const s = open();
      s.hub.emit('refresh', 'users', 1, 0, 0);
      s.answer('User/2/*', [users[0]]);
      expect(s.component.users.length).toBe(1);
    });

    it('ignores the notices of other screens and of other companies', () => {
      const s = open();
      s.hub.emit('refresh', 'costumer', 1, 0, 0);
      s.hub.emit('refresh', 'users', 9, 0, 0);
      s.http.expectNone(r => r.url.includes('/User/'));
    });

    it('puts into the window what another person saved, without the password', () => {
      const s = open();
      s.component.fillModal('edit', { ...users[1] }); s.answer('Business/2', []);
      s.hub.emit('refresh', 'users', 1, 4, 0);
      s.answer('User/2/*', [users[0], { ...users[1], nombre: 'Carlos Ramírez R.', contrasena: 'secreto' }]);
      expect(s.component.form.get('nombre')!.value).toBe('Carlos Ramírez R.');
      expect(s.component.form.get('contrasena')!.value).toBeFalsy();
    });

    it('ends the session when the person was disabled', () => {
      const s = open();
      s.hub.emit('refresh', 'users', 1, 0, 0);
      s.answer('User/2/*', [{ ...users[0], habilitado: false }]);
      expect(alertSpy).toHaveBeenCalledWith(s.service.messages.userDisabled);
      s.http.expectOne(r => r.url.endsWith('Login/Logout'));
    });

    it('shows the error when the reload fails', () => {
      const s = open();
      s.hub.emit('refresh', 'users', 1, 0, 0);
      s.answer('User/2/*', { errors: [{ detail: 'Caído' }] }, { status: 400 });
      expect(s.swal).toHaveBeenCalled();
    });
  });

  it('closes the real-time connection with the screen', () => {
    const s = open();
    s.fixture.destroy();
    expect(s.hub.stop).toHaveBeenCalled();
  });
});
