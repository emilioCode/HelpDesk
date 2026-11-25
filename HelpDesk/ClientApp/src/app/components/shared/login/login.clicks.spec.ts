import { afterEach, describe, expect, it, vi } from 'vitest';
import { LoginComponent } from './login.component';
import { openScreen, typeIn } from '../../../testing/screen-testing';

describe('LoginComponent, using the form', () => {
  afterEach(() => vi.restoreAllMocks());

  it('typing the user and the password and pressing the button signs in', () => {
    const s = openScreen(LoginComponent, null);
    s.service.setUser = vi.fn() as any;
    s.detect();
    typeIn(s, '[formcontrolname=user]', 'admin');
    typeIn(s, '[formcontrolname=pwd]', 'secreto');
    (s.element.querySelector('button[type=submit]') as HTMLButtonElement).click(); s.detect();
    const req = s.http.expectOne(r => r.url.endsWith('api/Login'));
    expect(req.request.body).toMatchObject({ CuentaUsuario: 'admin', Contrasena: 'secreto' });
    req.flush({ id: 2, token: 'tok' });
    expect(s.service.setUser).toHaveBeenCalled();
  });

  it('submitting the form with Enter does the same', () => {
    const s = openScreen(LoginComponent, null);
    s.service.setUser = vi.fn() as any;
    s.detect();
    typeIn(s, '[formcontrolname=user]', 'admin');
    typeIn(s, '[formcontrolname=pwd]', 'secreto');
    s.element.querySelector('form')!.dispatchEvent(new Event('submit')); s.detect();
    s.http.expectOne(r => r.url.endsWith('api/Login')).flush({ id: 2, token: 'tok' });
    expect(s.service.setUser).toHaveBeenCalled();
  });
});
