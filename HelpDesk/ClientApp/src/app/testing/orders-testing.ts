import { ActivatedRoute } from '@angular/router';
import { WaitingComponent } from '../components/shared/content/waiting/waiting.component';
import { openScreen, USERS } from './screen-testing';

// What the tests of the orders screen share: an order, a list of orders and a way to open the screen as a person.
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
