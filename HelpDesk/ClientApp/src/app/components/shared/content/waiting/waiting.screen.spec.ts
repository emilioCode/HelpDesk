import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { ActivatedRoute } from '@angular/router';
import { WaitingComponent } from './waiting.component';
import { openScreen, USERS } from '../../../../testing/screen-testing';

export const order = (over: any = {}) => ({
  id: 1, noSecuencia: 5, idEmpresa: 1, tipoSolicitud: 'Servicio Taller', tipoServicio: 'Contrato', estado: 'en Proceso', idUsuario: 4, idCliente: 2,
  cliente: 'Bufete Almonte', descripcion: 'Laptop con pantalla dañada', aprobadoPor: null,
  fechaCreacion: '2026-10-04T00:00:00', fechaInicio: '2026-10-05T00:00:00', horaInicio: '09:30:00', fechaTermino: null, horaTermino: null, ...over
});

export const orders = [
  order(),
  order({ id: 2, noSecuencia: 9, tipoSolicitud: 'Servicio a Domicilio', tipoServicio: 'Renta', estado: 'Abierto', idUsuario: 0, cliente: 'Ferretería', fechaInicio: null, horaInicio: null }),
  order({ id: 3, noSecuencia: 3, estado: 'Completado', idUsuario: 4, aprobadoPor: 9, cliente: 'Clínica', fechaTermino: '2026-10-06T00:00:00', horaTermino: '17:00:00' }),
  order({ id: 4, noSecuencia: 4, estado: 'Completado', idUsuario: 4, aprobadoPor: null, cliente: 'Colegio' })
];

export function openOrders(data: any, user: any = USERS.admin, answers: Record<string, unknown> = {}) {
  const s = openScreen(WaitingComponent, user, [{ provide: ActivatedRoute, useValue: { snapshot: { data } } }]);
  const id = user.id;
  s.detect();
  s.answer(new RegExp('api/Ticket/' + id + '/(\\*|unique)$'), answers['tickets'] ?? orders);
  s.detect();
  return s;
}

describe('WaitingComponent, the lists', () => {
  const alertSpy = vi.fn();
  beforeEach(() => { alertSpy.mockClear(); vi.stubGlobal('alert', alertSpy); vi.stubGlobal('confirm', vi.fn(() => true)); });
  afterEach(() => { vi.restoreAllMocks(); vi.unstubAllGlobals(); });

  it('lists every order on the screen of all the orders', () => {
    const s = openOrders({ mode: 'all', title: 'Ordenes de servicio' });
    expect(s.component.tickets.length).toBe(4);
    expect(s.element.querySelectorAll('#example2 tbody tr').length).toBe(4);
    expect(s.element.querySelectorAll('.hd-order-card').length).toBe(4);
    expect(s.component.title).toBe('Ordenes de servicio');
  });

  it('lists what was not assigned yet on the screen of pending orders, and only of the asked type', () => {
    const s = openOrders({ title: 'Pendientes', requestType: 'onSite' });
    expect(s.component.mode).toBe('pending');
    expect(s.component.tickets.map((t: any) => t.id)).toEqual([2]);
    const w = openOrders({ title: 'Pendientes', requestType: 'workshop' });
    expect(w.component.tickets.length).toBe(0);
    expect(w.element.textContent).toContain('No hay órdenes que mostrar');
  });

  it('lists the orders waiting for approval on the finished screen', () => {
    const s = openOrders({ mode: 'finished', title: 'Completadas' });
    expect(s.component.tickets.map((t: any) => t.id)).toEqual([4]);
  });

  it('lists the orders of the technician that were not approved in the inbox', () => {
    const s = openOrders({ mode: 'inbox', title: 'Buzón' }, USERS.technician);
    expect(s.component.tickets.map((t: any) => t.id)).toEqual([1, 2, 4]);
  });

  it('keeps a technician out of every list but the inbox', () => {
    const s = openScreen(WaitingComponent, USERS.technician, [{ provide: ActivatedRoute, useValue: { snapshot: { data: { mode: 'all' } } } }]);
    expect(alertSpy).toHaveBeenCalledWith(s.service.messages.noPermissionToAccess);
    s.detect();
  });

  it('filters the list with the search box', () => {
    const s = openOrders({ mode: 'all' });
    s.component.search = 'Clínica'; s.detect();
    expect(s.element.querySelectorAll('#example2 tbody tr').length).toBe(1);
  });

  it('shows the error when the list cannot be loaded', () => {
    const s = openScreen(WaitingComponent, USERS.admin, [{ provide: ActivatedRoute, useValue: { snapshot: { data: { mode: 'all' } } } }]);
    s.detect();
    s.answer(/api\/Ticket\/2\/\*$/, { errors: [{ detail: 'Caído' }] }, { status: 400 });
    expect(s.swal).toHaveBeenCalledWith('Warning', 'Caído', 'warning');
  });

  describe('real-time notices', () => {
    it('reloads the list when an order of the company changes', () => {
      const s = openOrders({ mode: 'all' });
      s.hub.emit('refresh', 'ticket', 1, 0, 0);
      s.answer(/api\/Ticket\/2\/\*$/, [orders[0]]);
      expect(s.component.tickets.length).toBe(1);
    });

    it('shows again the order that is open when another person changed it', () => {
      const s = openOrders({ mode: 'all' });
      s.component.fillModal('edit', { ...orders[0] });
      s.settle();
      s.hub.emit('refresh', 'ticket', 1, 4, 1);
      s.answer(/api\/Ticket\/2\/\*$/, [order({ descripcion: 'Cambiada' })]);
      expect(s.component.ticket.descripcion).toBe('Cambiada');
      s.settle();
    });

    it('closes the window when the order that was open is not in the list any more', () => {
      const s = openOrders({ mode: 'finished' });
      s.component.fillModal('edit', { ...orders[3] });
      s.settle();
      const closed = vi.fn(); s.element.querySelector('#btnClose')!.addEventListener('click', closed);
      s.hub.emit('refresh', 'ticket', 1, 0, 4);
      s.answer(/api\/Ticket\/2\/\*$/, []);
      expect(closed).toHaveBeenCalled();
    });

    it('reloads the customers on the screen of all the orders when one changes', () => {
      const s = openOrders({ mode: 'all' });
      s.hub.emit('refresh', 'costumer', 1, 0, 0);
      s.answer(/api\/Costumer\/2\/\*$/, [{ id: 2, nombre: 'Bufete' }]);
      expect(s.component.costumers.length).toBe(1);
    });

    it('shows the error when a reload fails', () => {
      const s = openOrders({ mode: 'all' });
      s.hub.emit('refresh', 'ticket', 1, 0, 0);
      s.answer(/api\/Ticket\/2\/\*$/, {}, { status: 500 });
      expect(s.swal).toHaveBeenCalled();
      s.hub.emit('refresh', 'costumer', 1, 0, 0);
      s.answer(/api\/Costumer\/2\/\*$/, {}, { status: 500 });
    });

    it('ends the session when the person was disabled', () => {
      const s = openOrders({ mode: 'all' });
      s.hub.emit('refresh', 'session', 1, 2, 0);
      expect(alertSpy).toHaveBeenCalled();
      s.http.expectOne(r => r.url.endsWith('Login/Logout'));
    });
  });

  it('closes the real-time connection with the screen', () => {
    const s = openOrders({ mode: 'all' });
    s.fixture.destroy();
    expect(s.hub.stop).toHaveBeenCalled();
  });
});
