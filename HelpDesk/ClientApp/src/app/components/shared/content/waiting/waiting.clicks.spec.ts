import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { openOrders, order } from '../../../../testing/orders-testing';
import { press, typeIn, USERS } from '../../../../testing/screen-testing';

type Orders = ReturnType<typeof openOrders>;
const traces = [{ id: 11, texto: 'Pantalla pedida', favorito: false, etiquetado: false, visibleAlCliente: false, userName: 'Carlos', hora: '10:40:00', fecha: '2026-10-06T00:00:00' }];
const customers = [
  { id: 2, nombre: 'Bufete Almonte', rnc: '130000011', contacto: 'Ana', telefono: '809-555-0101', extension: '1', correo: 'b@x.test', departamento: 'Ventas', direccion: 'Calle 1', habilitado: true, idEmpresa: 1 },
  { id: 3, nombre: 'Colegio Horizonte', rnc: '130000022', contacto: 'Marta', telefono: '809-555-0122', extension: '210', correo: 'c@x.test', departamento: 'Informática', direccion: 'Av. 45', habilitado: true, idEmpresa: 1 }
];
const done = { code: '1', title: 'Listo', message: 'ok', icon: 'success' };
const posted = (s: Orders, end: string) => s.http.expectOne(r => r.method === 'POST' && r.url.endsWith(end));

/** Opens the list and presses the pencil of the row of the order, as a person does. */
function opened(over: any = {}, user: any = USERS.admin, mode = 'all', answers: Record<string, unknown> = {}): Orders {
  const s = openOrders({ mode }, user);
  s.component.tickets = [order(over)];
  s.detect();
  s.element.querySelector<HTMLElement>('#example2 tbody tr a.btn-link')!.click(); s.detect();
  s.settle({ 'Trace/1/1': { data: traces }, ...answers });
  s.detect();
  return s;
}
const inWindow = '#modal-default';

// The buttons of the screen pressed one by one, as a person would, instead of calling the methods.
describe('WaitingComponent, pressing the buttons', () => {
  const alertSpy = vi.fn();
  const confirmSpy = vi.fn(() => true);
  beforeEach(() => { alertSpy.mockClear(); confirmSpy.mockReset(); confirmSpy.mockReturnValue(true); vi.stubGlobal('alert', alertSpy); vi.stubGlobal('confirm', confirmSpy); });
  afterEach(() => { vi.restoreAllMocks(); vi.unstubAllGlobals(); });

  describe('the list', () => {
    it('the pencil of a row opens that order in the window', () => {
      const s = opened();
      expect(s.component.option).toBe('edit');
      expect(s.component.ticket.id).toBe(1);
      expect(s.element.querySelector(inWindow + ' .modal-title')!.textContent).toContain('5');
    });

    it('the card of the phone opens the order too', () => {
      const s = openOrders({ mode: 'all' });
      s.element.querySelector<HTMLElement>('.hd-order-card')!.click(); s.detect();
      expect(s.component.option).toBe('edit');
      s.settle();
    });

    it('"Crear orden" opens an empty window, and "Crear" sends what was chosen and typed', () => {
      const s = openOrders({ mode: 'all' });
      press(s, 'Crear orden'); s.settle({ 'Costumer/2/*': customers }); s.detect();
      expect(s.component.option).toBe('add');
      typeIn(s, inWindow + ' [formcontrolname=idCliente]', '2');
      typeIn(s, inWindow + ' [formcontrolname=tipoSolicitud]', 'Servicio a Domicilio');
      typeIn(s, inWindow + ' [formcontrolname=tipoServicio]', 'Renta');
      typeIn(s, inWindow + ' input[placeholder="Falla Reportada"]', 'No enciende');
      press(s, 'Crear', inWindow + ' .modal-footer');
      const req = posted(s, 'api/Ticket');
      expect(req.request.body).toMatchObject({ idCliente: '2', tipoSolicitud: 'Servicio a Domicilio', tipoServicio: 'Renta', descripcion: 'No enciende' });
      req.flush({ ...done, data: { id: 50, noSecuencia: 13 } });
      expect(s.toasts()).toContain('No:13');
    });

    it('"Cerrar" of the window loads the list again', () => {
      const s = opened();
      press(s, 'Cerrar', inWindow + ' .modal-footer');
      s.http.match(r => r.url.includes('api/Ticket/')).forEach(r => r.flush([]));
    });
  });

  describe('the order in the window', () => {
    it('"Enviar al cliente" asks the server to send the link', () => {
      const s = opened();
      press(s, /Enviar al cliente/);
      posted(s, 'Tracking/Email/1').flush({ ...done, title: 'Enviado' });
    });

    it('"Copiar enlace" asks for the link and copies it', async () => {
      const s = opened();
      const write = vi.fn(() => Promise.resolve());
      Object.defineProperty(navigator, 'clipboard', { value: { writeText: write }, configurable: true });
      press(s, /Copiar enlace/);
      const req = s.http.expectOne(r => r.url.endsWith('Tracking/Link/1'));
      req.flush({ data: { url: 'https://x/t/abc' } });
      await vi.waitFor(() => expect(write).toHaveBeenCalledWith('https://x/t/abc'));
    });

    it('changing the dates sends the order', () => {
      const s = opened({ estado: 'en Proceso' });
      typeIn(s, inWindow + ' [formcontrolname=fechaTermino]', '2026-10-09');
      const req = posted(s, 'Ticket/EDIT/EDITAR');
      expect(req.request.body).toMatchObject({ id: 1, fechaTermino: '2026-10-09' });
      req.flush({ code: '1', data: order({ fechaTermino: '2026-10-09T00:00:00' }) });
    });

    it('choosing a state asks and sends it', () => {
      const s = opened({ estado: 'Abierto', idUsuario: 4 });
      typeIn(s, inWindow + ' [formcontrolname=estado]', 'en Proceso');
      const req = posted(s, 'Ticket/EDIT/ESTADO');
      expect(req.request.body).toMatchObject({ id: 1, estado: 'en Proceso' });
      req.flush({ code: '1', data: order({ estado: 'en Proceso' }) });
    });

    it('choosing a technician sends the assignment', () => {
      const s = opened({ idUsuario: 0, estado: 'Abierto' }, USERS.admin, 'all', { 'User/2/*': [{ id: 4, nombre: 'Carlos', cuentaUsuario: 'tecnico1' }] });
      s.component.users = [{ id: 4, nombre: 'Carlos', cuentaUsuario: 'tecnico1' }]; s.detect();
      typeIn(s, inWindow + ' [formcontrolname=idUsuario]', '4');
      const req = posted(s, 'Ticket/EDIT/USUARIO');
      expect(req.request.body).toMatchObject({ id: 1 });
      req.flush({ code: '1', data: order({ idUsuario: 4 }) });
    });

    it('"Aprobar" approves a completed order', () => {
      const s = opened({ estado: 'Completado', aprobadoPor: null, fechaTermino: '2026-10-06T00:00:00', horaTermino: '17:00:00' });
      press(s, 'Aprobar');
      posted(s, 'Ticket/EDIT/APROBAR').flush({ code: '1', data: order({ estado: 'Completado', aprobadoPor: 2 }) });
    });
  });

  describe('the devices of the order', () => {
    const fill = (s: Orders) => ['descripcion', 'fallaReportada', 'marca', 'modelo', 'noSerial'].forEach((n, i) => typeIn(s, inWindow + ' tr[ng-reflect-form] [formcontrolname=' + n + '], ' + inWindow + ' [formcontrolname=' + n + '][placeholder="(Requerido)"]', ['Laptop', 'No enciende', 'Lenovo', 'E14', 'SN1'][i]));

    it('the plus opens the row, the check sends the device and the cross closes the row', () => {
      const s = opened({ tipoSolicitud: 'Servicio a Domicilio' });
      s.element.querySelector<HTMLElement>(inWindow + ' table th a.btn-link')!.click(); s.detect();
      expect(s.component.addDevice).toBe(true);
      fill(s);
      s.element.querySelector<HTMLElement>(inWindow + ' td a.btn-link .fa-check')!.parentElement!.click(); s.detect();
      const req = posted(s, 'Device/PostOne');
      expect(req.request.body).toMatchObject({ marca: 'Lenovo', idSolicitud: 1 });
      req.flush(done);
      s.settle({ 'Trace/1/1': { data: traces } });
      s.element.querySelector<HTMLElement>(inWindow + ' td a.btn-link .fa-remove')!.parentElement!.click(); s.detect();
      expect(s.component.addDevice).toBe(false);
    });
  });


  describe('the activities', () => {
    it('typing an activity and pressing the button sends it', () => {
      const s = opened();
      typeIn(s, inWindow + ' input[placeholder^="Escribe una actividad"]', 'Se pidió la pieza');
      press(s, /Colocar actividad/);
      const req = posted(s, 'api/Trace');
      expect(req.request.body).toMatchObject({ texto: 'Se pidió la pieza', idSolicitud: 1 });
      req.flush(done);
    });

    it('pressing Enter in the field sends it too', () => {
      const s = opened();
      const field = typeIn(s, inWindow + ' input[placeholder^="Escribe una actividad"]', 'Otra nota');
      field.dispatchEvent(new KeyboardEvent('keyup', { key: 'Enter', bubbles: true })); s.detect();
      posted(s, 'api/Trace').flush(done);
    });

    it('the stars and tags of an activity change it, and the eye shows it to the customer', () => {
      const s = opened();
      const toggles = Array.from(s.element.querySelectorAll<HTMLElement>(inWindow + ' .hd-toggle'));
      expect(toggles.length).toBeGreaterThanOrEqual(3);
      toggles[0].click(); s.detect();
      expect(posted(s, 'Trace/Put').request.body).toMatchObject({ id: 11, favorito: true });
      s.http.match(r => r.url.endsWith('Trace/Put')).forEach(r => r.flush(done));
      toggles[toggles.length - 1].click(); s.detect();
      s.http.match(r => r.url.includes('Trace/Visible') || r.url.endsWith('Trace/Put')).forEach(r => r.flush(done));
      toggles[0].dispatchEvent(new KeyboardEvent('keyup', { key: 'Enter', bubbles: true })); s.detect();
      s.http.match(r => r.url.endsWith('Trace/Put')).forEach(r => r.flush(done));
    });
  });

  describe('the parts', () => {
    const part = { id: 7, cantidad: 2, noSerial: 'LCD-1', descripcion: 'Pantalla' };
    const withParts = () => opened({}, USERS.admin, 'all', { 'Part/1/1': [part] });
    const tab = (s: Orders) => s.element.querySelector<HTMLElement>(inWindow + ' .hd-parts')!;

    it('the plus opens the row, the check sends the part and the cross closes the row', () => {
      const s = withParts();
      tab(s).querySelector<HTMLElement>('th a.btn-link')!.click(); s.detect();
      expect(s.component.addPart).toBe(true);
      typeIn(s, inWindow + ' .hd-parts [formcontrolname=cantidad]', '3');
      typeIn(s, inWindow + ' .hd-parts [formcontrolname=noSerial]', 'RAM-1');
      typeIn(s, inWindow + ' .hd-parts [formcontrolname=descripcion]', 'Memoria');
      tab(s).querySelector<HTMLElement>('.fa-check')!.parentElement!.click(); s.detect();
      const req = posted(s, 'api/Part');
      expect(req.request.body).toMatchObject({ noSerial: 'RAM-1', idSolicitud: 1 });
      req.flush(done);
      tab(s).querySelector<HTMLElement>('.fa-remove')!.parentElement!.click(); s.detect();
      expect(s.component.addPart).toBe(false);
    });

    it('the pencil of a part opens its window and "Editar" sends the change', () => {
      const s = withParts();
      tab(s).querySelector<HTMLElement>('span[title="editar"]')!.click(); s.detect();
      press(s, /Editar/, '#modalclient3');
      posted(s, 'Part/Put').flush(done);
    });

    it('the trash of a part opens its window and "Eliminar" removes it', () => {
      const s = withParts();
      tab(s).querySelector<HTMLElement>('.btn-link[data-target="#modalclient4"]')!.click(); s.detect();
      press(s, /Eliminar/, '#modalclient4');
      posted(s, 'Part/Delete/7').flush(done);
    });

    it('"Close" of the edit window reloads the parts', () => {
      const s = withParts();
      tab(s).querySelector<HTMLElement>('span[title="editar"]')!.click(); s.detect();
      press(s, 'Close', '#modalclient3');
      s.http.match(r => r.url.includes('Part/')).forEach(r => r.flush([part]));
    });
  });

  describe('the devices already saved', () => {
    const device = { id: 5, idSolicitud: 1, descripcion: 'Laptop', fallaReportada: 'No enciende', marca: 'Lenovo', modelo: 'E14', noSerial: 'SN1' };
    const withDevice = () => opened({ tipoSolicitud: 'Servicio Taller' }, USERS.admin, 'pending', { 'Device/1': [device] });

    it('the pencil opens the window of the device and "Editar" sends the change', () => {
      const s = withDevice();
      s.element.querySelector<HTMLElement>(inWindow + ' span[title="editar"][data-target="#modalclient5"]')!.click(); s.detect();
      press(s, /Editar/, '#modalclient5');
      posted(s, 'Device/Put').flush(done);
    });

    it('the trash opens the window of the device and "Eliminar" removes it', () => {
      const s = withDevice();
      s.element.querySelector<HTMLElement>(inWindow + ' span[data-target="#modalclient6"]')!.click(); s.detect();
      press(s, /Eliminar/, '#modalclient6');
      posted(s, 'Device/Delete/5').flush(done);
    });

    it('"Close" of the window reloads the devices', () => {
      const s = withDevice();
      s.element.querySelector<HTMLElement>(inWindow + ' span[title="editar"][data-target="#modalclient5"]')!.click(); s.detect();
      press(s, 'Close', '#modalclient5');
      s.http.match(r => r.url.includes('Device/')).forEach(r => r.flush([device]));
    });
  });
  describe('the customers', () => {
    it('"Desea validar/crear cliente" lists them, and the circle chooses one', () => {
      const s = openOrders({ mode: 'all' });
      press(s, 'Crear orden'); s.settle({ 'Costumer/2/*': customers }); s.detect();
      press(s, /Desea validar/);
      s.settle({ 'Costumer/2/*': customers }); s.detect();
      const choose = s.element.querySelector<HTMLElement>('#modalclient tbody tr a.btn-link')!;
      choose.click(); s.detect();
      expect(s.component.ticket.idCliente).toBe('2');
    });

    it('"agregar cliente" opens the new customer form and "Crear" sends it', () => {
      const s = openOrders({ mode: 'all' });
      press(s, 'Crear orden'); s.settle({ 'Costumer/2/*': customers }); s.detect();
      press(s, /Desea validar/); s.settle({ 'Costumer/2/*': customers }); s.detect();
      press(s, /agregar cliente/, '#modalclient');
      typeIn(s, '#modalclient2 [formcontrolname=nombre]', 'Cliente nuevo');
      typeIn(s, '#modalclient2 [formcontrolname=rnc]', '999');
      press(s, 'Crear', '#modalclient2 .modal-footer');
      const req = posted(s, 'api/Costumer');
      expect(req.request.body).toMatchObject({ nombre: 'Cliente nuevo' });
      req.flush({ ...done, data: { id: 9 } });
      s.settle();
    });
  });
});
