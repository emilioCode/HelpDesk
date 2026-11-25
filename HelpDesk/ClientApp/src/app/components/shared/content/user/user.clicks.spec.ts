import { afterEach, describe, expect, it, vi } from 'vitest';
import { UserComponent } from './user.component';
import { openScreen, press, typeIn, USERS } from '../../../../testing/screen-testing';

const users = [
  { id: 2, nombre: 'Ana Pérez', numDocumento: '001-1', cuentaUsuario: 'admin', correo: 'ana@demo.test', acceso: 'ADMINISTRADOR', idEmpresa: 1, habilitado: true, image: null },
  { id: 4, nombre: 'Carlos Ramírez', numDocumento: '001-3', cuentaUsuario: 'tecnico1', correo: 'carlos@demo.test', acceso: 'TECNICO', idEmpresa: 1, habilitado: true, image: 'YWJj' }
];

function open(user: any = USERS.admin) {
  const s = openScreen(UserComponent, user);
  s.answer('User/' + user.id + '/*', users);
  s.detect();
  return s;
}
const rowOf = (s: ReturnType<typeof open>, text: string) => Array.from(s.element.querySelectorAll('tbody tr')).find(r => r.textContent!.includes(text))!;

// The buttons of the screen pressed one by one, as a person would, instead of calling the methods.
describe('UserComponent, pressing the buttons', () => {
  afterEach(() => vi.restoreAllMocks());

  it('"Crear usuario" opens an empty form, and "Crear" sends what was typed', () => {
    const s = open();
    press(s, 'Crear usuario'); s.answer('Business/2', []); s.detect();
    expect(s.component.option).toBe('add');
    typeIn(s, '[formcontrolname=nombre]', 'Nuevo Usuario');
    typeIn(s, '[formcontrolname=numDocumento]', '9');
    typeIn(s, '[formcontrolname=cuentaUsuario]', 'nuevo1');
    s.answer('Login/1/nuevo1', true);
    typeIn(s, '[formcontrolname=acceso]', 'TECNICO');
    typeIn(s, '[formcontrolname=contrasena]', '123456');
    press(s, 'Crear', '.modal-footer');
    const req = s.http.expectOne(r => r.url.endsWith('api/User') && r.method === 'POST');
    expect(req.request.body).toMatchObject({ nombre: 'Nuevo Usuario', cuentaUsuario: 'nuevo1', contrasena: '123456' });
    req.flush({ code: '1', title: 'Listo', message: 'ok', icon: 'success' });
    expect(s.toasts()).toContain('Listo: ok');
  });

  it('checks the account name when the field changes', () => {
    const s = open();
    press(s, 'Crear usuario'); s.answer('Business/2', []); s.detect();
    typeIn(s, '[formcontrolname=cuentaUsuario]', 'admin');
    s.answer('Login/1/admin', false);
    s.detect();
    expect(s.component.renderHTML1).toContain('has-warning');
  });

  it('the pencil of a row opens that user and "Guardar" sends the change', () => {
    const s = open();
    rowOf(s, 'Carlos Ramírez').querySelector<HTMLElement>('a.btn-link')!.click(); s.detect(); s.answer('Business/2', []); s.detect();
    expect(s.component.option).toBe('edit');
    typeIn(s, '[formcontrolname=nombre]', 'Carlos R.');
    press(s, 'Guardar', '.modal-footer');
    const req = s.http.expectOne(r => r.url.endsWith('User/Put/2'));
    expect(req.request.body).toMatchObject({ id: 4, nombre: 'Carlos R.' });
    req.flush({ code: '1', title: 'Listo', message: 'ok', icon: 'success' });
  });

  it('the switch of a row disables the user', () => {
    const s = open();
    rowOf(s, 'Carlos Ramírez').querySelectorAll<HTMLElement>('a.btn-link')[1].click(); s.detect();
    const req = s.http.expectOne(r => r.method === 'POST');
    expect(req.request.url).toContain('User/Put');
    req.flush({ code: '1', title: 'Listo', message: 'ok', icon: 'success' });
  });

  it('"Eliminar" takes the photo off, and choosing a file puts one', async () => {
    const s = open();
    rowOf(s, 'Carlos Ramírez').querySelector<HTMLElement>('a.btn-link')!.click(); s.detect(); s.answer('Business/2', []); s.detect();
    press(s, 'Eliminar');
    expect(s.component.user.image).toBeNull();
    const input = s.element.querySelector<HTMLInputElement>('input[type="file"]')!;
    Object.defineProperty(input, 'files', { value: { item: () => new File(['abc'], 'a.png', { type: 'image/png' }), length: 1 }, configurable: true });
    input.dispatchEvent(new Event('change'));
    await vi.waitFor(() => expect(s.component.user.image).toBe('YWJj'));
  });

  it('"Cerrar" reloads the list', () => {
    const s = open();
    press(s, 'Crear usuario'); s.answer('Business/2', []); s.detect();
    press(s, 'Cerrar', '.modal-footer');
    s.answer('User/2/*', users);
  });

  it('shows no create button below administrator', () => {
    const s = openScreen(UserComponent, USERS.technician); s.detect();
    expect(s.element.textContent).not.toContain('Crear usuario');
    s.settle();
  });
});
