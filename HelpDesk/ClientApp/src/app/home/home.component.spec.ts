import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { HomeComponent } from './home.component';
import { openScreen, USERS } from '../testing/screen-testing';

const numbers = { enCurso: 3, pendientes: 2, cerrados: 7, clientes: 7, sinAsignar: 2, talleres: 1, domicilios: 1, completadas: 4 };

function open(user: any = USERS.admin) {
  const s = openScreen(HomeComponent, user);
  s.answer('Ticket/numbersOfTickets/' + user.id, { data: numbers });
  s.detect();
  return s;
}

describe('HomeComponent', () => {
  const alertSpy = vi.fn();
  beforeEach(() => { alertSpy.mockClear(); vi.stubGlobal('alert', alertSpy); });
  afterEach(() => { vi.restoreAllMocks(); vi.unstubAllGlobals(); });

  it('shows the numbers of the orders', () => {
    const s = open();
    expect(s.component.numbers).toEqual(numbers);
    expect(s.service.isLoading).toBe(false);
  });

  it('shows the cards of the home', () => {
    const s = open();
    expect(s.element.querySelectorAll('.small-box').length).toBeGreaterThan(3);
  });

  it('writes the error to the console when the numbers cannot be loaded', () => {
    const s = openScreen(HomeComponent, USERS.admin);
    s.detect();
    s.answer('Ticket/numbersOfTickets/2', { errors: [] }, { status: 500 });
    expect(s.service.isLoading).toBe(false);
  });

  describe('real-time notices', () => {
    it('reloads the numbers when an order of the person changes', () => {
      const s = open(USERS.technician);
      s.hub.emit('refresh', 'ticket', 1, 4, 0);
      s.answer('Ticket/numbersOfTickets/4', { data: { ...numbers, enCurso: 9 } });
      expect(s.component.numbers.enCurso).toBe(9);
    });

    it('reloads them for any change of the company when the person is a moderator or more', () => {
      const s = open();
      s.hub.emit('refresh', 'ticket', 1, 99, 0);
      s.answer('Ticket/numbersOfTickets/2', { data: numbers });
    });

    it('reloads them when a customer changes', () => {
      const s = open(USERS.technician);
      s.hub.emit('refresh', 'customer', 1, 0, 0);
      s.answer('Ticket/numbersOfTickets/4', { data: numbers });
    });

    it('does not reload for a technician when the order is of someone else', () => {
      const s = open(USERS.technician);
      s.hub.emit('refresh', 'ticket', 1, 99, 0);
      s.http.expectNone(() => true);
    });

    it('does not reload for another company', () => {
      const s = open();
      s.hub.emit('refresh', 'ticket', 9, 2, 0);
      s.http.expectNone(() => true);
    });

    it('keeps the numbers when the reload fails', () => {
      const s = open();
      s.hub.emit('refresh', 'ticket', 1, 2, 0);
      s.answer('Ticket/numbersOfTickets/2', {}, { status: 500 });
      expect(s.component.numbers).toEqual(numbers);
    });

    it('ends the session when the person was disabled', () => {
      const s = open();
      s.hub.emit('refresh', 'session', 1, 2, 0);
      expect(alertSpy).toHaveBeenCalled();
      s.http.expectOne(r => r.url.endsWith('Login/Logout'));
    });
  });

  it('closes the real-time connection with the screen', () => {
    const s = open();
    s.fixture.destroy();
    expect(s.hub.stop).toHaveBeenCalled();
  });
});
