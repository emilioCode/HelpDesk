import { afterEach, describe, expect, it, vi } from 'vitest';
import { CostumerComponent } from './costumer.component';
import { openScreen, press, typeIn, USERS } from '../../../../testing/screen-testing';

const customers = [
  { id: 1, nombre: 'Farmacia San Rafael', rnc: '130000011', contacto: 'Ana', telefono: '809-555-0101', extension: '1', correo: 'f@x.test', departamento: 'Ventas', direccion: 'Calle 1', habilitado: true, idEmpresa: 1 },
  { id: 2, nombre: 'Colegio Horizonte', rnc: '130000022', contacto: 'Marta', telefono: '809-555-0122', extension: '210', correo: 'c@x.test', departamento: 'Informática', direccion: 'Av. 45', habilitado: false, idEmpresa: 1 }
];

function open(user = USERS.admin) {
  const s = openScreen(CostumerComponent, user);
  s.answer('Costumer/' + user.id + '/*', customers);
  s.detect();
  return s;
}

const rowOf = (s: ReturnType<typeof open>, text: string) => Array.from(s.element.querySelectorAll('tbody tr')).find(r => r.textContent!.includes(text))!;

// The buttons of the screen pressed one by one, as a person would, instead of calling the methods.
describe('CostumerComponent, pressing the buttons', () => {
  afterEach(() => vi.restoreAllMocks());

  it('"Crear cliente" opens an empty form and "Crear" sends what was typed', () => {
    const s = open();
    press(s, 'Crear cliente'); s.answer('Business/2', []); s.detect();
    expect(s.component.option).toBe('add');
    typeIn(s, '[formcontrolname=nombre]', 'Cliente nuevo');
    typeIn(s, '[formcontrolname=rnc]', '999');
    press(s, 'Crear', '.modal-footer');
    const req = s.http.expectOne(r => r.url.endsWith('api/Costumer') && r.method === 'POST');
    expect(req.request.body).toMatchObject({ nombre: 'Cliente nuevo', rnc: '999', idEmpresa: 1 });
    req.flush({ code: '1', title: 'Listo', message: 'ok', icon: 'success' });
    expect(s.toasts()).toContain('Listo: ok');
  });

  it('keeps "Crear" disabled until the form is valid', () => {
    const s = open();
    press(s, 'Crear cliente'); s.answer('Business/2', []); s.detect();
    const create = s.element.querySelector<HTMLButtonElement>('.modal-footer .btn-primary')!;
    expect(create.disabled).toBe(true);
    typeIn(s, '[formcontrolname=nombre]', 'Cliente nuevo');
    typeIn(s, '[formcontrolname=rnc]', '999');
    expect(create.disabled).toBe(false);
  });

  it('the pencil of a row opens the customer and "Guardar" sends the change', () => {
    const s = open();
    rowOf(s, 'Farmacia San Rafael').querySelector<HTMLElement>('a.btn-link')!.click(); s.detect();
    s.answer('Business/2', []); s.detect();
    expect(s.component.option).toBe('edit');
    expect((s.element.querySelector('[formcontrolname=nombre]') as HTMLInputElement).value).toBe('Farmacia San Rafael');
    typeIn(s, '[formcontrolname=nombre]', 'Farmacia Norte');
    press(s, 'Guardar', '.modal-footer');
    const req = s.http.expectOne(r => r.url.endsWith('Costumer/Put/2'));
    expect(req.request.body).toMatchObject({ id: 1, nombre: 'Farmacia Norte' });
    req.flush({ code: '1', title: 'Listo', message: 'ok', icon: 'success' });
  });

  it('the switch of a row enables or disables the customer', () => {
    const s = open();
    rowOf(s, 'Colegio Horizonte').querySelectorAll<HTMLElement>('a.btn-link')[1].click(); s.detect();
    const req = s.http.expectOne(r => r.method === 'POST');
    expect(req.request.url).toContain('Costumer');
    req.flush({ code: '1', title: 'Listo', message: 'ok', icon: 'success' });
  });

  it('"Cerrar" reloads the list', () => {
    const s = open();
    press(s, 'Crear cliente'); s.answer('Business/2', []); s.detect();
    press(s, 'Cerrar', '.modal-footer');
    s.answer('Costumer/2/*', customers);
  });

  it('shows no create button to a technician', () => {
    const s = open(USERS.technician);
    expect(s.element.textContent).not.toContain('Crear cliente');
  });
});
