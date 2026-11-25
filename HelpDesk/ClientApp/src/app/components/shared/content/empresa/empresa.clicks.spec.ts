import { afterEach, describe, expect, it, vi } from 'vitest';
import { EmpresaComponent } from './empresa.component';
import { openScreen, press, typeIn, USERS } from '../../../../testing/screen-testing';

const companies = [
  { id: 1, razonSocial: 'Demo Company', sectorComercial: 'Servicios', rnc: '101', telefono: '809', noAutorizacion: '', correo: 's@demo.test', contrasena: null, secuenciaticket: '13', url: '', host: 'mailpit', port: 1025, direccion: 'Av. 1', condicionesTaller: '', condicionesDomicilio: '', habilitado: true, image: 'YWJj' }
];

function open(user: any = USERS.root) {
  const s = openScreen(EmpresaComponent, user);
  s.answer('Business/' + user.id, companies);
  s.detect();
  return s;
}

// The buttons of the screen pressed one by one, as a person would, instead of calling the methods.
describe('EmpresaComponent, pressing the buttons', () => {
  const alertSpy = vi.fn();
  afterEach(() => { vi.restoreAllMocks(); vi.unstubAllGlobals(); });

  it('"Crear entidad" opens an empty form, and "Crear" sends what was typed', () => {
    vi.stubGlobal('alert', alertSpy);
    const s = open();
    press(s, 'Crear entidad'); s.detect();
    expect(s.component.option).toBe('add');
    typeIn(s, '[formcontrolname=razonSocial]', 'Nueva SRL');
    typeIn(s, '[formcontrolname=secuenciaticket]', '1');
    press(s, 'Crear', '.modal-footer');
    const req = s.http.expectOne(r => r.url.endsWith('api/Business/1') && r.method === 'POST');
    expect(req.request.body).toMatchObject({ razonSocial: 'Nueva SRL' });
    req.flush({ code: '1', title: 'Listo', message: 'ok', icon: 'success' });
    expect(s.toasts()).toContain('Listo: ok');
  });

  it('the pencil opens the company and "Guardar" sends the change', () => {
    const s = open();
    s.element.querySelector<HTMLElement>('tbody tr a.btn-link')!.click(); s.detect();
    expect(s.component.option).toBe('edit');
    typeIn(s, '[formcontrolname=razonSocial]', 'Demo Company SRL');
    press(s, 'Guardar', '.modal-footer');
    const req = s.http.expectOne(r => r.url.endsWith('Business/Put/1'));
    expect(req.request.body).toMatchObject({ id: 1, razonSocial: 'Demo Company SRL' });
    req.flush({ code: '1', title: 'Listo', message: 'ok', icon: 'success' });
  });

  it('the switch of the row disables the company', () => {
    const s = open();
    s.element.querySelectorAll<HTMLElement>('tbody tr a.btn-link')[1].click(); s.detect();
    const req = s.http.expectOne(r => r.method === 'POST');
    expect(req.request.url).toContain('Business/Put');
    req.flush({ code: '1', title: 'Listo', message: 'ok', icon: 'success' });
  });

  it('"Eliminar" takes the logo off and a chosen file puts one', async () => {
    const s = open();
    s.element.querySelector<HTMLElement>('tbody tr a.btn-link')!.click(); s.detect();
    press(s, 'Eliminar');
    expect(s.component.empresa.image).toBeNull();
    const input = s.element.querySelector<HTMLInputElement>('input[type="file"]')!;
    Object.defineProperty(input, 'files', { value: { item: () => new File(['abc'], 'l.png', { type: 'image/png' }), length: 1 }, configurable: true });
    input.dispatchEvent(new Event('change'));
    await vi.waitFor(() => expect(s.component.empresa.image).toBe('YWJj'));
  });

  it('"Cerrar" reloads the list', () => {
    const s = open();
    press(s, 'Crear entidad'); s.detect();
    press(s, 'Cerrar', '.modal-footer');
    s.answer('Business/1', companies);
  });
});
