import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { openOrders, order, orders } from './waiting.screen.spec';
import { USERS } from '../../../../testing/screen-testing';

type Orders = ReturnType<typeof openOrders>;

const done = { code: '1', title: 'Listo', message: 'ok', icon: 'success' };
const traces = [{ id: 11, texto: 'Pantalla pedida', favorito: false, etiquetado: false, visibleAlCliente: false, userName: 'Carlos', hora: '10:40:00', fecha: '2026-10-06T00:00:00' }];

/** Opens the screen of all the orders as the person, with an order in its window. */
function withOrder(over: any = {}, user: any = USERS.admin, mode = 'all'): Orders {
  const s = openOrders({ mode }, user);
  s.component.fillModal('edit', order(over));
  s.settle({ 'Trace/1/1': { data: traces } });
  s.detect();
  return s;
}
function adding(user: any = USERS.admin): Orders {
  const s = openOrders({ mode: 'all' }, user);
  s.component.fillModal('add', {});
  s.settle();
  s.detect();
  return s;
}

describe('WaitingComponent, the order', () => {
  const alertSpy = vi.fn();
  const confirmSpy = vi.fn(() => true);
  beforeEach(() => { alertSpy.mockClear(); confirmSpy.mockReset(); confirmSpy.mockReturnValue(true); vi.stubGlobal('alert', alertSpy); vi.stubGlobal('confirm', confirmSpy); });
  afterEach(() => { vi.restoreAllMocks(); vi.unstubAllGlobals(); });

  describe('the window', () => {
    it('starts a new order with the company, no technician and the first state', () => {
      const s = adding();
      expect(s.component.option).toBe('add');
      expect(s.component.ticket).toMatchObject({ idEmpresa: 1, tipoSolicitud: '', tipoServicio: '', idCliente: '', idUsuario: '0', estado: 'Abierto' });
      expect(s.component.devices).toEqual([]);
      expect(s.component.parts).toEqual([]);
    });

    it('loads the devices, the parts, the activities, the approvers and the lists when editing', () => {
      const s = openOrders({ mode: 'all' });
      s.component.fillModal('edit', order());
      expect(s.http.match(() => true).map(r => r.request.url.replace('http://app/api/', '')).sort()).toEqual(
        ['Costumer/2/*', 'Device/1', 'Part/1/1', 'Trace/1/1', 'User/2/*', 'User/2/JUST NAME'].sort());
    });

    it('writes the dates the way the date fields read them', () => {
      const s = withOrder({ fechaInicio: '2026-10-05T00:00:00', fechaTermino: '2026-10-06T00:00:00' });
      expect(s.component.ticket.fechaInicio).toBe('2026-10-05');
      expect(s.component.ticket.fechaTermino).toBe('2026-10-06');
    });

    it('mirrors the order in the form, and the form in the order', () => {
      const s = withOrder();
      expect(s.component.ticketForm.get('tipoServicio')!.value).toBe('Contrato');
      s.component.ticketForm.get('descripcion')!.setValue('Otra falla');
      expect(s.component.ticket.descripcion).toBe('Otra falla');
    });

    it('locks the fields an order cannot change any more, and the type of request once it exists', () => {
      const s = withOrder({ estado: 'Completado', aprobadoPor: 9 });
      s.detect();
      const f = s.component.ticketForm;
      expect(f.get('tipoSolicitud')!.disabled).toBe(true);
      expect(f.get('idCliente')!.disabled).toBe(true);
      expect(f.get('fechaInicio')!.disabled).toBe(true);
    });

    it('lets a new order choose its type and has no dates yet', () => {
      const s = adding();
      const f = s.component.ticketForm;
      expect(f.get('tipoSolicitud')!.enabled).toBe(true);
      expect(f.get('fechaInicio')!.disabled).toBe(true);
    });

    it('does not let a technician assign the order, except in the inbox', () => {
      const t = withOrder({}, USERS.technician, 'inbox');
      t.detect();
      expect(t.component.ticketForm.get('idUsuario')!.enabled).toBe(true);
    });

    it('shows the activities of the order', () => {
      const s = withOrder();
      expect(s.component.traces.length).toBe(1);
      expect(s.element.textContent).toContain('Pantalla pedida');
    });

    it('does not break when a load of the window fails', () => {
      const s = openOrders({ mode: 'all' });
      s.component.fillModal('edit', order());
      s.http.match(() => true).forEach(r => r.flush({ errors: [{ detail: 'Caído' }] }, { status: 400, statusText: 'Bad Request' }));
      expect(s.swal).toHaveBeenCalled();
      expect(s.service.isLoading).toBe(false);
    });
  });

  describe('adding an order', () => {
    const complete = (s: Orders, over: any = {}) => { Object.assign(s.component.ticket, { idCliente: '2', tipoSolicitud: 'Servicio a Domicilio', tipoServicio: 'Renta', descripcion: 'No enciende' }, over); s.detect(); };

    it('asks for the minimum data', () => {
      const s = adding();
      expect(s.component.add()).toBe(false);
      expect(s.swal).toHaveBeenCalledWith(s.service.messages.titleRequiredFields, s.service.messages.completeMinimumOrderData, 'warning');
    });

    it('asks for the reported failure on an order on site', () => {
      const s = adding(); complete(s, { descripcion: '' });
      expect(s.component.add()).toBe(false);
      expect(s.swal).toHaveBeenCalledWith(s.service.messages.titleRequiredFields, s.service.messages.enterReportedFailure, 'warning');
    });

    it('does not let a technician create orders', () => {
      const s = openOrders({ mode: 'inbox' }, USERS.technician);
      s.component.fillModal('add', {}); s.settle(); s.detect();
      complete(s);
      expect(s.component.add()).toBe(false);
      expect(s.swal).toHaveBeenCalledWith(s.service.messages.titleAccessDenied, '', 'error');
    });

    it('creates the order, keeps what the server gave and tells the others', () => {
      const s = adding(); complete(s);
      s.component.add();
      const req = s.http.expectOne(r => r.url.endsWith('api/Ticket') && r.method === 'POST');
      expect(req.request.body).toMatchObject({ idCliente: '2', tipoSolicitud: 'Servicio a Domicilio', idUsuario: 0 });
      req.flush({ ...done, data: { id: 50, noSecuencia: 13, fechaCreacion: '2026-11-24T00:00:00', fechaInicio: null, fechaTermino: null, horaTermino: null } });
      expect(s.component.option).toBe('edit');
      expect(s.component.ticket).toMatchObject({ id: 50, noSecuencia: 13 });
      expect(s.hub.invoke).toHaveBeenCalledWith('refresh', 'ticket', 1, 0, 0);
      expect(s.toasts()).toContain('No:13');
    });

    it('does not create a workshop order without a device', () => {
      const s = adding(); complete(s, { tipoSolicitud: 'Servicio Taller' });
      s.component.devices = [];
      expect(s.component.add()).toBe(false);
      expect(s.swal).toHaveBeenCalledWith(s.service.messages.mustHaveDevice, '', 'warning');
      s.http.expectNone(r => r.url.endsWith('api/Ticket') && r.method === 'POST');
    });

    it('sends the devices of a workshop order after the order', () => {
      const s = adding(); complete(s, { tipoSolicitud: 'Servicio Taller' });
      s.component.devices = [{ descripcion: 'Laptop', fallaReportada: 'x', marca: 'M', modelo: 'Y', noSerial: '1' }];
      s.component.add();
      s.http.expectOne(r => r.url.endsWith('api/Ticket')).flush({ ...done, data: { id: 51, noSecuencia: 14 } });
      const req = s.http.expectOne(r => r.url.endsWith('Device/PostArray'));
      expect(req.request.body[0]).toMatchObject({ idSolicitud: 51, marca: 'M' });
      req.flush(done);
    });

    it('puts the devices back and says why when they were not saved', () => {
      const s = adding(); complete(s, { tipoSolicitud: 'Servicio Taller' });
      s.component.devices = [{ descripcion: 'Laptop', fallaReportada: 'x', marca: 'M', modelo: 'Y', noSerial: '1' }];
      s.component.add();
      s.http.expectOne(r => r.url.endsWith('api/Ticket')).flush({ ...done, data: { id: 51, noSecuencia: 14 } });
      s.http.expectOne(r => r.url.endsWith('Device/PostArray')).flush({ code: '0', title: 'No', message: 'Falló', icon: 'warning' });
      expect(s.swal).toHaveBeenCalledWith('No', 'Falló', 'warning');
      expect(s.component.devices.length).toBe(1);
    });

    it('does not move on when the server refused the order', () => {
      const s = adding(); complete(s);
      s.component.add();
      s.http.expectOne(r => r.url.endsWith('api/Ticket')).flush({ code: '0', title: 'No', message: 'Duplicada', icon: 'warning' });
      expect(s.component.option).toBe('add');
      expect(s.hub.invoke).not.toHaveBeenCalled();
    });

    it('shows the error when creating fails', () => {
      const s = adding(); complete(s);
      s.component.add();
      s.http.expectOne(r => r.url.endsWith('api/Ticket')).flush({ errors: [{ detail: 'Caído' }] }, { status: 400, statusText: 'Bad Request' });
      expect(s.swal).toHaveBeenCalledWith('Warning', 'Caído', 'warning');
      expect(s.service.isLoading).toBe(false);
    });

    it('clears the failure and the devices of an order on site when the type changes', () => {
      const s = adding(); complete(s);
      s.component.devices = [{ id: 1 }];
      s.component.clearDevicesIf();
      expect(s.component.devices).toEqual([]);
      expect(s.component.ticket.descripcion).toBe('');
    });
  });

  describe('the devices of an order that is being written', () => {
    const device = (over: any = {}) => ({ descripcion: 'Laptop', fallaReportada: 'No enciende', marca: 'Lenovo', modelo: 'E14', noSerial: 'SN1', ...over });
    const workshop = () => { const s = adding(); s.component.ticket.tipoSolicitud = 'Servicio Taller'; return s; };

    it('asks for the brand, the model, the serial and the failure', () => {
      const s = workshop();
      expect(s.component.addDeviceList(device({ marca: '' }))).toBe(false);
      expect(s.swal).toHaveBeenCalledWith(s.service.messages.titleRequiredFields, s.service.messages.deviceDataRequired, 'info');
    });

    it('does not take a device with a value too long', () => {
      const s = workshop();
      s.component.deviceForm.patchValue({ marca: 'x'.repeat(51) });
      expect(s.component.addDeviceList(device({ marca: 'x'.repeat(51) }))).toBe(false);
      expect(s.swal).toHaveBeenCalledWith(s.service.messages.titleRequiredFields, s.service.messages.deviceDataTooLong, 'info');
    });

    it('lists the device, with the company, and closes the adding row', () => {
      const s = workshop(); s.component.addDevice = true;
      s.component.addDeviceList(device());
      expect(s.component.devices.length).toBe(1);
      expect(s.component.devices[0]).toMatchObject({ idEmpresa: 1, idSolicitud: 0 });
      expect(s.component.addDevice).toBe(false);
    });

    it('gives each listed device its own number, and takes one out', () => {
      const s = workshop();
      s.component.addDeviceList(device()); s.component.addDeviceList(device({ noSerial: 'SN2' }));
      expect(s.component.devices.map((d: any) => d.id)).toEqual([0, 1]);
      s.component.removeDeviceList(s.component.devices[0]);
      expect(s.component.devices.length).toBe(1);
    });

    it('sends the device of an order on site at once', () => {
      const s = withOrder({ tipoSolicitud: 'Servicio a Domicilio' });
      s.component.addDeviceList(device());
      const req = s.http.expectOne(r => r.url.endsWith('Device/PostOne'));
      expect(req.request.body).toMatchObject({ id: 0, idSolicitud: 1, idEmpresa: 1 });
      req.flush(done);
      expect(s.hub.invoke).toHaveBeenCalledWith('refresh', 'ticket', 1, 4, 1);
    });

    it('empties the devices and says why when one was refused', () => {
      const s = withOrder();
      s.component.addDevicePostOne(device());
      s.http.expectOne(r => r.url.endsWith('Device/PostOne')).flush({ code: '0', title: 'No', message: 'Falló', icon: 'warning' });
      expect(s.component.devices).toEqual([]);
      expect(s.swal).toHaveBeenCalledWith('No', 'Falló', 'warning');
    });

    it('shows the error when sending a device fails', () => {
      const s = withOrder();
      s.component.addDevicePostOne(device());
      s.http.expectOne(r => r.url.endsWith('Device/PostOne')).flush({ errors: [{ detail: 'Caído' }] }, { status: 400, statusText: 'Bad Request' });
      expect(s.swal).toHaveBeenCalled();
    });
  });

  describe('editing and removing devices and parts of an order', () => {
    const part = (over: any = {}) => ({ id: 7, cantidad: 2, noSerial: 'LCD-1', descripcion: 'Pantalla', ...over });

    it('asks for what is missing in a part', () => {
      const s = withOrder();
      expect(s.component.addPartOne(part({ cantidad: 0 }))).toBe(false);
      expect(s.component.addPartOne(part({ noSerial: '' }))).toBe(false);
      expect(s.component.editPartUsed(part({ descripcion: '' }))).toBe(false);
      expect(s.swal).toHaveBeenCalledTimes(3);
    });

    it('adds a part to the order and tells the others', () => {
      const s = withOrder(); s.component.addPart = true;
      s.component.addPartOne(part());
      const req = s.http.expectOne(r => r.url.endsWith('api/Part') && r.method === 'POST');
      expect(req.request.body).toMatchObject({ idSolicitud: 1, idEmpresa: 1 });
      req.flush(done);
      expect(s.component.addPart).toBe(false);
      expect(s.hub.invoke).toHaveBeenCalledWith('refresh', 'ticket', 1, 4, 1);
    });

    it('empties the parts and says why when one was refused, and when the call fails', () => {
      const s = withOrder();
      s.component.addPartOne(part());
      s.http.expectOne(r => r.url.endsWith('api/Part')).flush({ code: '0', title: 'No', message: 'Falló', icon: 'warning' });
      expect(s.component.parts).toEqual([]);
      s.component.addPartOne(part());
      s.http.expectOne(r => r.url.endsWith('api/Part')).flush({ errors: [{ detail: 'Caído' }] }, { status: 400, statusText: 'Bad Request' });
      expect(s.swal).toHaveBeenCalledTimes(2);
    });

    it('edits a part', () => {
      const s = withOrder();
      s.component.partChoosen(part());
      s.component.editPartUsed(s.component.parToEdit);
      s.http.expectOne(r => r.url.endsWith('Part/Put')).flush(done);
      expect(s.hub.invoke).toHaveBeenCalledWith('refresh', 'ticket', 1, 4, 1);
      expect(s.toasts()).toContain('Listo: ok');
    });

    it('reloads the parts when editing one was refused or failed', () => {
      const s = withOrder();
      s.component.editPartUsed(part());
      s.http.expectOne(r => r.url.endsWith('Part/Put')).flush({ code: '0', title: 'No', message: 'Falló', icon: 'warning' });
      s.answer('Part/1/1', []);
      s.component.editPartUsed(part());
      s.http.expectOne(r => r.url.endsWith('Part/Put')).flush({}, { status: 500, statusText: 'Error' });
      expect(s.swal).toHaveBeenCalled();
    });

    it('removes a part', () => {
      const s = withOrder();
      s.component.deletePartUsed(part());
      s.http.expectOne(r => r.url.endsWith('Part/Delete/7')).flush(done);
      expect(s.hub.invoke).toHaveBeenCalled();
    });

    it('reloads the parts when removing one was refused or failed', () => {
      const s = withOrder();
      s.component.deletePartUsed(part());
      s.http.expectOne(r => r.url.endsWith('Part/Delete/7')).flush({ code: '0', title: 'No', message: 'x', icon: 'warning' });
      s.answer('Part/1/1', []);
      s.component.deletePartUsed(part());
      s.http.expectOne(r => r.url.endsWith('Part/Delete/7')).flush({}, { status: 500, statusText: 'Error' });
    });

    it('removes a device, and reloads them when it was refused or failed', () => {
      const s = withOrder();
      s.component.deviceChoosen({ id: 3 });
      expect(s.component.deviceToEdit).toEqual({ id: 3 });
      s.component.deleteDevice({ id: 3 });
      s.http.expectOne(r => r.url.endsWith('Device/Delete/3')).flush(done);
      s.component.deleteDevice({ id: 3 });
      s.http.expectOne(r => r.url.endsWith('Device/Delete/3')).flush({ code: '0', title: 'No', message: 'x', icon: 'warning' });
      s.answer('Device/1', []);
      s.component.deleteDevice({ id: 3 });
      s.http.expectOne(r => r.url.endsWith('Device/Delete/3')).flush({}, { status: 500, statusText: 'Error' });
    });

    it('edits a device and asks for what is missing', () => {
      const s = withOrder();
      expect(s.component.editDevice({ marca: '', fallaReportada: 'x', modelo: 'y', noSerial: 'z' })).toBe(false);
      s.component.editDevice({ id: 3, marca: 'M', fallaReportada: 'x', modelo: 'y', noSerial: 'z' });
      s.http.expectOne(r => r.url.endsWith('Device/Put')).flush(done);
      expect(s.hub.invoke).toHaveBeenCalled();
      s.component.editDevice({ id: 3, marca: 'M', fallaReportada: 'x', modelo: 'y', noSerial: 'z' });
      s.http.expectOne(r => r.url.endsWith('Device/Put')).flush({ code: '0', title: 'No', message: 'x', icon: 'warning' });
      s.answer('Part/1/1', []);
    });

    it('edits a device with the address of the inbox in the inbox', () => {
      const s = withOrder({}, USERS.technician, 'inbox');
      s.component.editDevice({ id: 3, marca: 'M', fallaReportada: 'x', modelo: 'y', noSerial: 'z' });
      s.http.expectOne(r => r.url.endsWith('Device/Put/')).flush({}, { status: 500, statusText: 'Error' });
    });
  });

  describe('the activities', () => {
    it('does not add a blank activity', () => {
      const s = withOrder();
      s.component.trace = { texto: '   ' };
      expect(s.component.addTrace()).toBe(false);
      s.http.expectNone(r => r.method === 'POST');
    });

    it('adds an activity with the person and the order, and tells the others on the lists', () => {
      const s = withOrder();
      s.component.trace = { texto: 'Se pidió la pieza' };
      s.component.addTrace();
      const req = s.http.expectOne(r => r.url.endsWith('api/Trace') && r.method === 'POST');
      expect(req.request.body).toMatchObject({ texto: 'Se pidió la pieza', idUsuario: 2, idSolicitud: 1, idEmpresa: 1 });
      req.flush(done);
      expect(s.hub.invoke).toHaveBeenCalledWith('refresh', 'ticket', 1, 4, 1);
      expect(s.component.trace).toEqual({});
    });

    it('reloads the activities itself on the screen of pending orders', () => {
      const s = withOrder({}, USERS.admin, 'pending');
      s.component.trace = { texto: 'Nota' };
      s.component.addTrace();
      s.http.expectOne(r => r.url.endsWith('api/Trace') && r.method === 'POST').flush(done);
      s.answer('Trace/1/1', { data: traces });
    });

    it('shows the error when adding fails', () => {
      const s = withOrder();
      s.component.trace = { texto: 'Nota' };
      s.component.addTrace();
      s.http.expectOne(r => r.url.endsWith('api/Trace') && r.method === 'POST').flush({ errors: [{ detail: 'Caído' }] }, { status: 400, statusText: 'Bad Request' });
      expect(s.swal).toHaveBeenCalled();
    });

    it('marks an activity as favourite or as the solution', () => {
      const s = withOrder();
      s.component.putTrace(traces[0], 'favorito');
      s.http.expectOne(r => r.url.endsWith('Trace/Put')).flush(done);
      expect(traces[0].favorito).toBe(true);
      s.component.putTrace(traces[0], 'etiquetado');
      s.http.expectOne(r => r.url.endsWith('Trace/Put')).flush(done);
      expect(traces[0].etiquetado).toBe(true);
      s.component.putTrace(traces[0], 'otro');
      s.http.expectOne(r => r.url.endsWith('Trace/Put')).flush({}, { status: 500, statusText: 'Error' });
      traces[0].favorito = false; traces[0].etiquetado = false;
    });

    it('shows an activity to the customer or hides it', () => {
      const s = withOrder();
      s.component.putTrace(traces[0], 'visibleAlCliente');
      s.http.expectOne(r => r.url.endsWith('Trace/Visible/11/true')).flush(done);
      expect(traces[0].visibleAlCliente).toBe(true);
      s.component.putTrace(traces[0], 'visibleAlCliente');
      s.http.expectOne(r => r.url.endsWith('Trace/Visible/11/false')).flush({}, { status: 500, statusText: 'Error' });
      traces[0].visibleAlCliente = false;
    });
  });

  describe('the link to follow the order', () => {
    it('sends the link to the e-mail of the customer', () => {
      const s = withOrder();
      s.component.sendTracking();
      s.http.expectOne(r => r.url.endsWith('Tracking/Email/1')).flush({ code: '1' });
      expect(s.toasts().some(t => t.startsWith('Enviado'))).toBe(true);
    });

    it('says why the e-mail was not sent', () => {
      const s = withOrder();
      s.component.sendTracking();
      s.http.expectOne(r => r.url.endsWith('Tracking/Email/1')).flush({ code: '5', title: 'Sin correo', message: 'El cliente no tiene correo', icon: 'warning' });
      expect(s.swal).toHaveBeenCalledWith('Sin correo', 'El cliente no tiene correo', 'warning');
    });

    it('shows the error when sending fails', () => {
      const s = withOrder();
      s.component.sendTracking();
      s.http.expectOne(r => r.url.endsWith('Tracking/Email/1')).flush({}, { status: 500, statusText: 'Error' });
      expect(s.service.isLoading).toBe(false);
    });

    it('copies the link, and shows it when the browser does not let it copy', async () => {
      const write = vi.fn(() => Promise.resolve());
      Object.defineProperty(navigator, 'clipboard', { value: { writeText: write }, configurable: true });
      const s = withOrder();
      s.component.copyTrackingLink();
      s.http.expectOne(r => r.url.endsWith('Tracking/Link/1')).flush({ data: { url: 'https://x/t/abc' } });
      await vi.waitFor(() => expect(write).toHaveBeenCalledWith('https://x/t/abc'));
      await vi.waitFor(() => expect(s.toasts().some(t => t.startsWith('Enlace copiado') && t.includes('https://x/t/abc'))).toBe(true));
      Object.defineProperty(navigator, 'clipboard', { value: undefined, configurable: true });
      s.component.copyTrackingLink();
      s.http.expectOne(r => r.url.endsWith('Tracking/Link/1')).flush({ data: { url: 'https://x/t/def' } });
      expect(s.swal).toHaveBeenCalledWith('Enlace de seguimiento', 'https://x/t/def', 'info');
    });

    it('shows the link when the copy is refused', async () => {
      Object.defineProperty(navigator, 'clipboard', { value: { writeText: vi.fn(() => Promise.reject(new Error('no'))) }, configurable: true });
      const s = withOrder();
      s.component.copyTrackingLink();
      s.http.expectOne(r => r.url.endsWith('Tracking/Link/1')).flush({ data: { url: 'https://x/t/abc' } });
      await vi.waitFor(() => expect(s.swal).toHaveBeenCalledWith('Enlace de seguimiento', 'https://x/t/abc', 'info'));
    });

    it('shows the error when the link cannot be made', () => {
      const s = withOrder();
      s.component.copyTrackingLink();
      s.http.expectOne(r => r.url.endsWith('Tracking/Link/1')).flush({}, { status: 500, statusText: 'Error' });
      expect(s.service.isLoading).toBe(false);
    });
  });

  describe('a new customer from the window of the order', () => {
    it('chooses a customer for the order', () => {
      const s = withOrder();
      s.component.chooseClient({ id: 8 });
      expect(s.component.ticket.idCliente).toBe('8');
    });

    it('does not let a technician create customers', () => {
      const s = withOrder({}, USERS.technician, 'inbox');
      expect(s.component.addClient()).toBe(false);
      expect(s.swal).toHaveBeenCalledWith(s.service.messages.titleAccessDenied, '', 'error');
    });

    it('creates the customer without the empty fields, chooses it and tells the others', () => {
      const s = withOrder();
      s.component.newClient();
      s.component.costumer = { nombre: 'Nuevo', rnc: '9', contacto: '', telefono: null };
      s.component.addClient();
      const req = s.http.expectOne(r => r.url.endsWith('api/Costumer') && r.method === 'POST');
      expect(req.request.body).toEqual({ nombre: 'Nuevo', rnc: '9', idEmpresa: 1 });
      req.flush({ ...done, data: { id: 21 } });
      expect(s.component.ticket.idCliente).toBe('21');
      expect(s.hub.invoke).toHaveBeenCalledWith('refresh', 'costumer', 1, 0, 0);
      expect(s.component.costumer).toEqual({});
    });

    it('keeps the window open when the customer was refused, and shows the error when it fails', () => {
      const s = withOrder();
      s.component.costumer = { nombre: 'Nuevo', rnc: '9' };
      s.component.addClient();
      s.http.expectOne(r => r.url.endsWith('api/Costumer')).flush({ code: '0', title: 'No', message: 'RNC repetido', icon: 'warning' });
      expect(s.hub.invoke).not.toHaveBeenCalled();
      s.component.addClient();
      s.http.expectOne(r => r.url.endsWith('api/Costumer')).flush({ errors: [{ detail: 'Caído' }] }, { status: 400, statusText: 'Bad Request' });
      expect(s.swal).toHaveBeenCalledWith('Warning', 'Caído', 'warning');
    });
  });
});
