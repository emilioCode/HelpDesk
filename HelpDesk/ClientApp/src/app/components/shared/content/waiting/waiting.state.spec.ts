import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { openOrders, order } from './waiting.screen.spec';
import { USERS } from '../../../../testing/screen-testing';

type Orders = ReturnType<typeof openOrders>;
const traces = [{ id: 11, texto: 'Pantalla pedida', favorito: false, etiquetado: false, visibleAlCliente: false, userName: 'Carlos', hora: '10:40:00', fecha: '2026-10-06T00:00:00' }];

function withOrder(over: any = {}, user: any = USERS.admin, withTraces = true): Orders {
  const s = openOrders({ mode: 'all' }, user);
  s.component.fillModal('edit', order(over));
  s.settle({ 'Trace/1/1': { data: withTraces ? traces : [] } });
  s.detect();
  return s;
}
const posted = (s: Orders, option: string) => s.http.expectOne(r => r.url.endsWith('Ticket/EDIT/' + option) && r.method === 'POST');
const saved = (over: any = {}) => ({ code: '1', data: order({ horaInicio: '09:30:00.0000000', horaTermino: '17:00:00.5', ...over }) });

describe('WaitingComponent, the state of an order', () => {
  const alertSpy = vi.fn();
  const confirmSpy = vi.fn(() => true);
  beforeEach(() => { alertSpy.mockClear(); confirmSpy.mockReset(); confirmSpy.mockReturnValue(true); vi.stubGlobal('alert', alertSpy); vi.stubGlobal('confirm', confirmSpy); });
  afterEach(() => { vi.restoreAllMocks(); vi.unstubAllGlobals(); });

  describe('changing the state', () => {
    it('takes the order to the state that was chosen and shows what the server saved', () => {
      const s = withOrder({ estado: 'en Proceso' });
      s.component.putTicket('estado');
      posted(s, 'ESTADO').flush(saved({ estado: 'en Proceso' }));
      expect(s.hub.invoke).toHaveBeenCalledWith('refresh', 'ticket', 1, 4, 1);
      expect(s.component.ticket.horaInicio).toBe('09:30:00');
      expect(s.component.ticket.horaTermino).toBe('17:00:00');
      s.settle({ 'Trace/1/1': { data: traces } });
    });

    it('completes an order', () => {
      const s = withOrder({ estado: 'Completado' });
      s.component.putTicket('ESTADO');
      posted(s, 'ESTADO').flush(saved({ estado: 'Completado' }));
      s.settle({ 'Trace/1/1': { data: traces } });
    });

    it('asks before it reopens an order, and does nothing when the person says no', () => {
      const s = withOrder({ estado: 'Abierto' });
      confirmSpy.mockReturnValue(false);
      s.component.putTicket('ESTADO');
      expect(confirmSpy).toHaveBeenCalledWith(s.service.messages.confirmReopenTicket);
      posted(s, 'NOCHANGEPLEASE').flush({ code: '1', data: order({ estado: 'Abierto' }) });
      s.settle({ 'Trace/1/1': { data: traces } });
    });

    it('reopens the order when the person says yes', () => {
      const s = withOrder({ estado: 'Abierto' });
      s.component.putTicket('ESTADO');
      posted(s, 'ESTADO').flush({ code: '1', data: order({ estado: 'Abierto' }) });
      s.settle({ 'Trace/1/1': { data: traces } });
    });

    it('does not change the state of an order that has no technician, and says so', () => {
      const s = withOrder({ idUsuario: '0', estado: 'en Proceso' });
      s.component.putTicket('ESTADO');
      expect(alertSpy).toHaveBeenCalledWith(s.service.messages.ticketWithoutTechnician);
      posted(s, 'NOCHANGEPLEASE').flush({ code: '1', data: order() });
      s.settle({ 'Trace/1/1': { data: traces } });
    });

    it('puts the old state back and says why when the server refused the change', () => {
      const s = withOrder({ estado: 'en Proceso' });
      s.component.ticket.estado = 'Completado';
      s.component.putTicket('ESTADO');
      posted(s, 'ESTADO').flush({ code: '5', title: 'No se puede', message: 'Falta algo', icon: 'warning', data: order() });
      expect(s.swal).toHaveBeenCalledWith('No se puede', 'Falta algo', 'warning');
      s.settle({ 'Trace/1/1': { data: traces } });
    });

    it('keeps a state when the call fails, whatever the change was', () => {
      const s = withOrder({ estado: 'en Proceso' });
      s.component.ticket.estado = 'Completado';
      s.component.putTicket('ESTADO');
      posted(s, 'ESTADO').flush({ errors: [{ detail: 'Caído' }] }, { status: 400, statusText: 'Bad Request' });
      expect(s.component.ticket.estado).toBe('Completado');
    });

    it('puts the dates back when the call fails', () => {
      const s = withOrder({ estado: 'en Proceso', fechaInicio: '2026-10-05T00:00:00' });
      s.component.ticket.estado = 'Completado';
      s.component.ticket.fechaInicio = '2026-12-01';
      s.component.putTicket('ESTADO');
      posted(s, 'ESTADO').flush({ errors: [{ detail: 'Caído' }] }, { status: 400, statusText: 'Bad Request' });
      expect(s.component.ticket.fechaInicio).toBe('2026-10-05');
      expect(s.service.isLoading).toBe(false);
    });

    it('does nothing for a state it does not know', () => {
      const s = withOrder({ estado: 'Otro' });
      s.component.putTicket('ESTADO');
      posted(s, 'NOCHANGEPLEASE').flush({ code: '1', data: order() });
      s.settle({ 'Trace/1/1': { data: traces } });
    });
  });

  describe('assigning the technician', () => {
    it('waits until the order has a number', () => {
      const s = withOrder({ noSecuencia: null });
      expect(s.component.putTicket('USUARIO')).toBe(false);
      s.http.expectNone(r => r.method === 'POST');
    });

    it('asks, and assigns when the person says yes', () => {
      const s = withOrder();
      s.component.putTicket('USUARIO');
      expect(confirmSpy).toHaveBeenCalledWith(s.service.messages.confirmAssignTicket);
      posted(s, 'USUARIO').flush(saved());
      s.settle({ 'Trace/1/1': { data: traces } });
    });

    it('does not assign when the person says no', () => {
      const s = withOrder();
      confirmSpy.mockReturnValue(false);
      s.component.putTicket('USUARIO');
      posted(s, 'NOCHANGEPLEASE').flush(saved());
      s.settle({ 'Trace/1/1': { data: traces } });
    });
  });

  describe('approving', () => {
    it('asks, and records who approved when the person says yes', () => {
      const s = withOrder({ estado: 'Completado' });
      s.component.aprobar();
      expect(confirmSpy).toHaveBeenCalledWith(s.service.messages.confirmCompleteTicket);
      expect(s.component.ticket.aprobadoPor).toBe(2);
      posted(s, 'APROBAR').flush(saved({ estado: 'Completado', aprobadoPor: 2 }));
      s.settle({ 'Trace/1/1': { data: traces } });
    });

    it('does not approve when the person says no', () => {
      const s = withOrder({ estado: 'Completado' });
      confirmSpy.mockReturnValue(false);
      s.component.aprobar();
      expect(s.component.ticket.aprobadoPor).toBeNull();
      posted(s, 'NOCHANGEPLEASE').flush(saved());
      s.settle({ 'Trace/1/1': { data: traces } });
    });
  });

  describe('saving the changes of the fields', () => {
    it('saves what was edited', () => {
      const s = withOrder();
      s.component.edit();
      posted(s, 'EDITAR').flush(saved());
      s.settle({ 'Trace/1/1': { data: traces } });
    });

    it('refuses an option it does not know', () => {
      const s = withOrder();
      s.component.putTicket('OTRA');
      expect(s.swal).toHaveBeenCalledWith(s.service.messages.optionRequired, '', 'error');
      posted(s, 'NOCHANGEPLEASE').flush(saved());
      s.settle({ 'Trace/1/1': { data: traces } });
    });

    it('does not adjust a closed order, and tells the person', () => {
      const s = withOrder({ estado: 'Completado' });
      s.component.editAsAdministrator();
      expect(s.swal).toHaveBeenCalledWith(s.service.messages.titleClosedOrder, s.service.messages.closedOrderAdjustment, 'warning');
      s.http.expectNone(r => r.method === 'POST');
    });

    it('lets a moderator or more adjust an open order', () => {
      const s = withOrder({ estado: 'en Proceso' });
      s.component.editAsAdministrator();
      posted(s, 'EDITAR').flush(saved());
      s.settle({ 'Trace/1/1': { data: traces } });
    });

    it('does not let a technician adjust an order, and says so', () => {
      const s = openOrders({ mode: 'inbox' }, USERS.technician);
      s.component.fillModal('edit', order({ estado: 'en Proceso' }));
      s.settle({ 'Trace/1/1': { data: traces } });
      s.component.editAsAdministrator();
      expect(s.swal).toHaveBeenCalledWith(s.service.messages.titleAdjustmentNotApplied, s.service.messages.noAdminPermissionToAdjust, 'error');
      posted(s, 'NOCHANGEPLEASE').flush(saved());
      s.settle({ 'Trace/1/1': { data: traces } });
    });

    it('sends nothing when the order is not saved yet', () => {
      const s = openOrders({ mode: 'all' });
      s.component.fillModal('add', {}); s.settle();
      s.component.editAsAdministrator();
      s.http.expectNone(r => r.method === 'POST');
    });
  });

  describe('choosing a state in the window', () => {
    const choose = (s: Orders, estado: string) => { s.component.ticket.estado = estado; s.component.setEstatus(); };

    it('asks for an activity before completing an order that has none, and shows the tab of the activities', () => {
      const s = withOrder({ estado: 'en Proceso' }, USERS.admin, false);
      choose(s, 'Completado');
      expect(s.swal).toHaveBeenCalledWith(s.service.messages.titleRequiredActivities, s.service.messages.activityRequired, 'warning');
      expect(document.getElementById('li_timeline')!.classList.contains('active')).toBe(true);
      posted(s, 'NOCHANGEPLEASE').flush(saved());
      s.settle({ 'Trace/1/1': { data: [] } });
    });

    it('asks about the parts when completing, and takes the person to the parts if they say yes', async () => {
      const s = withOrder({ estado: 'en Proceso' });
      s.swal.mockResolvedValueOnce({ value: true } as any);
      choose(s, 'Completado');
      await vi.waitFor(() => expect(document.getElementById('li_settings')!.classList.contains('active')).toBe(true));
      posted(s, 'NOCHANGEPLEASE').flush(saved());
      s.settle({ 'Trace/1/1': { data: traces } });
    });

    it('completes the order when the person says it is complete', async () => {
      const s = withOrder({ estado: 'en Proceso' });
      s.swal.mockResolvedValueOnce({ dismiss: 'cancel' } as any);
      choose(s, 'Completado');
      await vi.waitFor(() => expect(posted(s, 'ESTADO')).toBeTruthy());
    });

    it('keeps the state when the person closes the question by clicking outside', async () => {
      const s = withOrder({ estado: 'en Proceso' });
      s.swal.mockResolvedValueOnce({ dismiss: 'overlay' } as any);
      choose(s, 'Completado');
      await vi.waitFor(() => expect(posted(s, 'NOCHANGEPLEASE')).toBeTruthy());
    });

    it('keeps the state for any other way of closing the question', async () => {
      const s = withOrder({ estado: 'en Proceso' });
      s.swal.mockResolvedValueOnce({ dismiss: 'esc' } as any);
      choose(s, 'Completado');
      await vi.waitFor(() => expect(posted(s, 'NOCHANGEPLEASE')).toBeTruthy());
    });

    it('changes to any other state at once', () => {
      const s = withOrder({ estado: 'Abierto' });
      choose(s, 'en Proceso');
      posted(s, 'ESTADO').flush(saved());
      s.settle({ 'Trace/1/1': { data: traces } });
    });
  });

  it('loads the people that can approve, and shows the error when it fails', () => {
    const s = openOrders({ mode: 'all' });
    s.component.fillModal('edit', order());
    s.settle({ 'User/2/JUST NAME': [{ id: 9, nombre: 'Ana' }] });
    expect(s.component.costumerAPs).toEqual([{ id: 9, nombre: 'Ana' }]);
    s.component.getUsersAP(2, 'JUST NAME');
    s.answer('User/2/JUST NAME', { errors: [{ detail: 'Caído' }] }, { status: 400 });
    expect(s.swal).toHaveBeenCalled();
  });
});
