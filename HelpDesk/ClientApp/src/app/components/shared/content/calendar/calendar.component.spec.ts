import { afterEach, describe, expect, it, vi } from 'vitest';
import { Calendar } from '@fullcalendar/core';
import { CalendarComponent } from './calendar.component';
import { openScreen, USERS } from '../../../../testing/screen-testing';

const event = (over: any = {}) => ({
  title: 'Servicio Taller No.5 | Estado: en Proceso', start: '2026-11-02T09:30:00', end: '2026-11-03T10:00:00',
  user: { nombre: 'Carlos Ramírez', acceso: 'TECNICO' },
  ticket: { estado: 'en Proceso', tipoSolicitud: 'Servicio Taller', aprobadoPor: null, noSecuencia: 5, idEmpresa: 1, descripcion: 'Laptop dañada' }, ...over
});

// The calendar of the library needs a real layout, which the test environment has not; what is checked is what the screen gives it.
function open(events: any[] = [event()]) {
  const render = vi.spyOn(Calendar.prototype, 'render').mockImplementation(() => { });
  const s = openScreen(CalendarComponent, USERS.admin);
  s.detect();
  s.answer('Event', { data: events });
  return { s, render };
}
const given = (s: any) => s.component.events as any[];

describe('CalendarComponent', () => {
  afterEach(() => vi.restoreAllMocks());

  it('asks for the events of the person and draws the calendar', () => {
    const { s, render } = open();
    expect(given(s).length).toBe(1);
    expect(render).toHaveBeenCalled();
    expect(s.service.isLoading).toBe(false);
  });

  describe('the color of an event', () => {
    const classOf = (ticket: any) => { const { s } = open([event({ ticket: { ...event().ticket, ...ticket } })]); return given(s)[0].classNames[0]; };
    it('is red for an open order', () => expect(classOf({ estado: 'Abierto' })).toBe('hd-ev-open'));
    it('is amber for an order in progress', () => expect(classOf({ estado: 'en Proceso' })).toBe('hd-ev-progress'));
    it('is blue for a completed order that was approved', () => expect(classOf({ estado: 'Completado', aprobadoPor: 'Ana' })).toBe('hd-ev-approved'));
    it('is green for a completed order that was not approved yet', () => expect(classOf({ estado: 'Completado', aprobadoPor: null })).toBe('hd-ev-done'));
  });

  describe('the address of the printable order', () => {
    const pathOf = (ticket: any) => { const { s } = open([event({ ticket: { ...event().ticket, ...ticket } })]); return given(s)[0].path; };
    it('is the order of service for an order on site', () => expect(pathOf({ tipoSolicitud: 'Servicio a Domicilio' })).toBe('http://app/OrderReport?value=5-1'));
    it('is the workshop order while it is not approved', () => expect(pathOf({ tipoSolicitud: 'Servicio Taller', aprobadoPor: null })).toBe('http://app/OrderReport1?value=5-1'));
    it('is the closed workshop order once approved', () => expect(pathOf({ tipoSolicitud: 'Servicio Taller', aprobadoPor: 'Ana' })).toBe('http://app/OrderReportF?value=5-1'));
  });

  describe('orders without dates', () => {
    it('are shown today and marked as on hold', () => {
      const { s } = open([event({ start: null, end: null })]);
      expect(given(s)[0].title).toBe('*(HOLD) Servicio Taller No.5 | Estado: en Proceso');
      expect(given(s)[0].start).toBeInstanceOf(Date);
      expect(given(s)[0].end).toBeInstanceOf(Date);
    });

    it('end today when they only have a start', () => {
      const { s } = open([event({ end: null })]);
      expect(given(s)[0].end).toBeInstanceOf(Date);
      expect(given(s)[0].title).not.toContain('HOLD');
    });

    it('all use the same reading of the clock, so their order does not change', () => {
      const { s } = open([event({ start: null, end: null }), event({ start: null, end: null })]);
      expect(given(s)[0].end.getTime()).toBe(given(s)[1].end.getTime());
    });
  });

  it('shows the error when the events cannot be loaded', () => {
    vi.spyOn(Calendar.prototype, 'render').mockImplementation(() => { });
    const s = openScreen(CalendarComponent, USERS.admin);
    s.detect();
    s.answer('Event', { errors: [{ detail: 'Caído' }] }, { status: 400 });
    expect(s.swal).toHaveBeenCalledWith('Warning', 'Caído', 'warning');
    expect(s.service.isLoading).toBe(false);
  });

  it('keeps the calendar until the screen goes away', () => {
    const destroy = vi.spyOn(Calendar.prototype, 'destroy').mockImplementation(() => { });
    const { s } = open();
    s.fixture.destroy();
    expect(destroy).toHaveBeenCalled();
  });

  it('draws the calendar again, and does not leave the old one, when the events arrive twice', () => {
    const destroy = vi.spyOn(Calendar.prototype, 'destroy').mockImplementation(() => { });
    const { s } = open();
    s.component.getEvents(2);
    s.answer('Event', { data: [event()] });
    expect(destroy).toHaveBeenCalled();
  });
});
