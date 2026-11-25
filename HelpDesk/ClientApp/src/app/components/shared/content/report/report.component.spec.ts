import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { ReportComponent } from './report.component';
import { openScreen, USERS } from '../../../../testing/screen-testing';

const rows = [
  { noSecuencia: 5, tipoSolicitud: 'Servicio Taller', tipoServicio: 'Contrato', estado: 'en Proceso', atendidoPor: 'Carlos, Ramírez', cliente: 'Bufete', fechaCreacion: '2026-10-04T00:00:00', fechaInicio: '2026-10-05T00:00:00', horaInicio: '09:30:00', fechaTermino: null, horaTermino: null, aprobadoPor: null },
  { noSecuencia: 9, tipoSolicitud: 'Servicio a Domicilio', tipoServicio: 'Renta', estado: 'Completado', atendidoPor: null, cliente: 'Ferretería', fechaCreacion: '2026-10-11T00:00:00', fechaInicio: '2026-10-12T00:00:00', horaInicio: '08:00:00', fechaTermino: '2026-10-13T00:00:00', horaTermino: '17:00:00', aprobadoPor: 'Ana, Pérez' }
];

const open = (user: any = USERS.admin) => { const s = openScreen(ReportComponent, user); s.detect(); return s; };

describe('ReportComponent', () => {
  const alertSpy = vi.fn();
  beforeEach(() => { alertSpy.mockClear(); vi.stubGlobal('alert', alertSpy); });
  afterEach(() => { vi.restoreAllMocks(); vi.unstubAllGlobals(); });

  it('starts with an empty search for the company of the person', () => {
    const s = open();
    expect(s.component.request).toEqual({ tipoSolicitud: '', idEmpresa: 1 });
    expect(s.component.requests).toEqual([]);
  });

  it('keeps out a person below administrator', () => {
    const s = open(USERS.technician);
    expect(alertSpy).toHaveBeenCalledWith(s.service.messages.noPermissionToAccess);
  });

  describe('searching', () => {
    it('asks for the data when nothing was chosen', () => {
      const s = open();
      expect(s.component.toSearch()).toBe(false);
      expect(s.swal).toHaveBeenCalledWith(s.service.messages.titleRequiredFields, expect.stringContaining('datos'), 'info');
      s.http.expectNone(() => true);
    });

    it('asks for both dates when only one was chosen and there is no type', () => {
      const s = open();
      s.component.request.fechaInicio = '2026-10-01';
      expect(s.component.toSearch()).toBe(false);
      expect(s.swal).toHaveBeenCalledWith(s.service.messages.titleRequiredFields, expect.stringContaining('fechas'), 'warning');
    });

    it('sends what was typed in the filters', () => {
      const s = open();
      const type = s.element.querySelector('select') as HTMLSelectElement;
      type.value = 'Servicio a Domicilio'; type.dispatchEvent(new Event('change'));
      const [from, to] = Array.from(s.element.querySelectorAll('input[type="date"]')) as HTMLInputElement[];
      from.value = '2026-10-01'; from.dispatchEvent(new Event('input'));
      to.value = '2026-10-31'; to.dispatchEvent(new Event('input'));
      s.component.toSearch();
      const req = s.http.expectOne(r => r.url.endsWith('Ticket/GetJsonTicket'));
      expect(req.request.body).toMatchObject({ tipoSolicitud: 'Servicio a Domicilio', fechaInicio: '2026-10-01', fechaTermino: '2026-10-31', idEmpresa: 1 });
      req.flush(rows);
    });

    it('empties the fields of the filters when they are cleared', () => {
      const s = open();
      s.component.request.fechaInicio = '2026-10-01'; s.detect();
      const from = s.element.querySelectorAll('input[type="date"]')[0] as HTMLInputElement;
      expect(from.value).toBe('2026-10-01');
      s.component.clear(); s.detect();
      expect(from.value).toBe('');
    });

    it('searches by type alone', () => {
      const s = open();
      s.component.request.tipoSolicitud = 'Servicio Taller';
      s.component.toSearch();
      const req = s.http.expectOne(r => r.url.endsWith('Ticket/GetJsonTicket'));
      expect(req.request.body).toMatchObject({ tipoSolicitud: 'Servicio Taller', idEmpresa: 1 });
      req.flush(rows);
      expect(s.component.requests.length).toBe(2);
      expect(s.service.isLoading).toBe(false);
    });

    it('searches by both dates', () => {
      const s = open();
      s.component.request.fechaInicio = '2026-10-01'; s.component.request.fechaTermino = '2026-10-31';
      s.component.toSearch();
      s.answer('Ticket/GetJsonTicket', rows);
      s.detect();
      expect(s.element.querySelectorAll('tbody tr').length).toBe(2);
    });

    it('shows the error when the search fails', () => {
      const s = open();
      s.component.request.tipoSolicitud = 'Servicio Taller';
      s.component.toSearch();
      s.answer('Ticket/GetJsonTicket', { errors: [{ detail: 'Caído' }] }, { status: 400 });
      expect(s.swal).toHaveBeenCalledWith('Warning', 'Caído', 'warning');
    });

    it('clears what was chosen', () => {
      const s = open();
      s.component.request.fechaInicio = 'x';
      s.component.clear();
      expect(s.component.request).toEqual({ tipoSolicitud: '', idEmpresa: 1 });
    });
  });

  describe('the spreadsheet', () => {
    it('writes one line per order, with the dates, the times and the names without commas', () => {
      const s = open();
      s.component.requests = rows;
      let link!: HTMLAnchorElement;
      vi.spyOn(HTMLAnchorElement.prototype, 'click').mockImplementation(function (this: HTMLAnchorElement) { link = this; });
      s.component.exportCSV();
      const text = decodeURIComponent(link.href.replace(/^data:text\/csv; charset=utf-8,/, ''));
      expect(text.charCodeAt(0)).toBe(0xFEFF);                     // so Excel reads the accents
      expect(text).toContain('NO. SECUENCIA');
      expect(text).toContain('="5"');                               // pasted as text, so it keeps its zeros
      expect(text).toContain('2026-10-05');
      expect(text).toContain('9:30 AM');
      expect(text).toContain('5:00 PM');
      expect(text).toContain('Carlos  Ramírez');                    // the comma of the name would break the columns
      expect(text.trim().split('\n').length).toBe(3);               // the header and two orders
      expect(link.download).toContain('reporte solicitudes');
    });
  });

  describe('real-time notices', () => {
    it('ends the session when the person was disabled', () => {
      const s = open();
      s.hub.emit('refresh', 'session', 1, 2, 0);
      expect(alertSpy).toHaveBeenCalledWith(s.service.messages.userDisabled);
      s.http.expectOne(r => r.url.endsWith('Login/Logout'));
    });

    it('ignores the notices about other people', () => {
      const s = open();
      s.hub.emit('refresh', 'session', 1, 9, 0);
      s.http.expectNone(() => true);
    });
  });

  it('closes the real-time connection with the screen', () => {
    const s = open();
    s.fixture.destroy();
    expect(s.hub.stop).toHaveBeenCalled();
  });
});
