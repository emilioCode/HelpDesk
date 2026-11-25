import { afterEach, describe, expect, it, vi } from 'vitest';
import { ReportComponent } from './report.component';
import { openScreen, press, typeIn, USERS } from '../../../../testing/screen-testing';

const rows = [
  { noSecuencia: 5, tipoSolicitud: 'Servicio Taller', tipoServicio: 'Contrato', estado: 'en Proceso', atendidoPor: 'Carlos, Ramírez', cliente: 'Bufete', fechaCreacion: '2026-10-04T00:00:00', fechaInicio: '2026-10-05T00:00:00', horaInicio: '09:30:00', fechaTermino: null, horaTermino: null, aprobadoPor: null }
];

describe('ReportComponent, pressing the buttons', () => {
  afterEach(() => vi.restoreAllMocks());

  it('"Buscar" sends the filters, "Exportar a Excel" appears with the rows, and "Limpiar" empties the filters', () => {
    const s = openScreen(ReportComponent, USERS.admin); s.detect();
    typeIn(s, 'select', 'Servicio Taller');
    press(s, 'Buscar');
    s.answer('Ticket/GetJsonTicket', rows); s.detect();
    expect(s.element.textContent).toContain('Bufete');
    const click = vi.spyOn(HTMLAnchorElement.prototype, 'click').mockImplementation(() => { });
    press(s, 'Exportar a Excel');
    expect(click).toHaveBeenCalled();
    press(s, 'Limpiar');
    expect((s.element.querySelector('select') as HTMLSelectElement).value).toBe('');
  });
});
