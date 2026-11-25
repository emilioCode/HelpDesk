import { afterEach, describe, expect, it, vi } from 'vitest';
import { ProfileComponent } from './profile.component';
import { openScreen, press, typeIn, USERS } from '../../../../testing/screen-testing';

const me = { id: 2, nombre: 'Ana Pérez', cuentaUsuario: 'admin', correo: 'ana@demo.test', idEmpresa: 1, acceso: 'ADMINISTRADOR', habilitado: true, contrasena: 'x' };

describe('ProfileComponent, pressing the buttons', () => {
  afterEach(() => vi.restoreAllMocks());

  it('"Guardar cambios" sends the new password', () => {
    const s = openScreen(ProfileComponent, USERS.admin);
    s.answer('User/2/UNIQUE', [me]);
    s.answer('Business/2', [{ id: 1, razonSocial: 'Demo Company', image: null }]);
    s.answer('Ticket/numbersOfTickets/2', { data: { enCurso: 3, pendientes: 2, cerrados: 7 } });
    s.detect();
    typeIn(s, '[formcontrolname=contrasena]', '123456');
    typeIn(s, '[formcontrolname=confirmacion]', '123456');
    press(s, 'Guardar cambios');
    const req = s.http.expectOne(r => r.url.endsWith('User/Put/2'));
    expect(req.request.body).toMatchObject({ id: 2, contrasena: '123456' });
    req.flush({ code: '1', title: 'Listo', message: 'ok', icon: 'success' });
    s.settle({ 'User/2/UNIQUE': [me], 'Business/2': [{ id: 1, image: null }] });
    expect(s.toasts()).toContain('Listo: ok');
  });
});
