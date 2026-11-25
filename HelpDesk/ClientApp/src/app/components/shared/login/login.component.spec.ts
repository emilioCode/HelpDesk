import { afterEach, describe, expect, it, vi } from 'vitest';
import { LoginComponent } from './login.component';
import { openScreen } from '../../../testing/screen-testing';

function open() {
  const s = openScreen(LoginComponent, null);
  s.service.setUser = vi.fn() as any;
  s.detect();
  return s;
}

describe('LoginComponent', () => {
  afterEach(() => vi.restoreAllMocks());

  it('asks for the user and the password', () => {
    const s = open();
    expect(s.component.form.valid).toBe(false);
    s.component.form.patchValue({ user: 'admin' });
    expect(s.component.form.valid).toBe(false);
    s.component.form.patchValue({ pwd: 'x' });
    expect(s.component.form.valid).toBe(true);
  });

  it('keeps the button disabled until both are typed', () => {
    const s = open();
    const button = s.element.querySelector('button[type=submit]') as HTMLButtonElement;
    expect(button.disabled).toBe(true);
    s.component.form.patchValue({ user: 'admin', pwd: 'x' }); s.detect();
    expect(button.disabled).toBe(false);
  });

  it('signs in with the account and the password, and keeps the person who comes back', () => {
    const s = open();
    s.component.form.patchValue({ user: 'admin', pwd: 'demo123' });
    s.component.singIn();
    expect(s.component.isLoading).toBe(true);
    const req = s.http.expectOne(r => r.url.endsWith('api/Login'));
    expect(req.request.body).toEqual({ CuentaUsuario: 'admin', Contrasena: 'demo123' });
    req.flush({ id: 2, token: 'tok' });
    expect(s.service.setUser).toHaveBeenCalledWith({ id: 2, token: 'tok' });
    expect(s.component.isLoading).toBe(false);
  });

  it('says access is denied when the server answers nothing for those data', () => {
    const s = open();
    s.component.form.patchValue({ user: 'admin', pwd: 'mal' });
    s.component.singIn();
    s.http.expectOne(r => r.url.endsWith('api/Login')).flush(null);
    expect(s.swal).toHaveBeenCalledWith('Acceso Denegado', expect.any(String), 'error');
    expect(s.service.setUser).not.toHaveBeenCalled();
  });

  it('shows the error when the server cannot be reached', () => {
    const s = open();
    s.component.form.patchValue({ user: 'admin', pwd: 'x' });
    s.component.singIn();
    s.http.expectOne(r => r.url.endsWith('api/Login')).error(new ProgressEvent('error'));
    expect(s.swal).toHaveBeenCalledWith(s.service.messages.titleError, s.service.messages.noConnection, 'error');
    expect(s.component.isLoading).toBe(false);
  });

  it('shows the message of a locked account', () => {
    const s = open();
    s.component.form.patchValue({ user: 'admin', pwd: 'x' });
    s.component.singIn();
    s.http.expectOne(r => r.url.endsWith('api/Login')).flush({ errors: [{ detail: 'Cuenta bloqueada' }] }, { status: 400, statusText: 'Bad Request' });
    expect(s.swal).toHaveBeenCalledWith('Warning', 'Cuenta bloqueada', 'warning');
  });
});
