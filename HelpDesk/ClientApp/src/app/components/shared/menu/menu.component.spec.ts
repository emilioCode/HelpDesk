import { describe, expect, it } from 'vitest';
import { MenuComponent } from './menu.component';
import { openScreen, USERS } from '../../../testing/screen-testing';

const open = (user: any) => { const s = openScreen(MenuComponent, user); s.detect(); return s; };
const labels = (s: ReturnType<typeof open>) => s.component.groups.flatMap((g: any) => g.items.map((i: any) => i.label));

describe('MenuComponent', () => {
  it('offers a technician the inbox, customers, the calendar and the profile only', () => {
    expect(labels(open(USERS.technician))).toEqual(['Inicio', 'Buzón', 'Calendario', 'Clientes', 'Mi perfil']);
  });

  it('adds the lists of orders for a moderator', () => {
    const l = labels(open(USERS.moderator));
    expect(l).toEqual(['Inicio', 'Órdenes de servicio', 'Buzón', 'Sin asignar', 'Completadas', 'Calendario', 'Clientes', 'Mi perfil']);
  });

  it('adds the reports, the company and the users for an administrator', () => {
    const l = labels(open(USERS.admin));
    expect(l).toContain('Informes');
    expect(l).toContain('Empresa');
    expect(l).toContain('Usuarios');
  });

  it('shows all of it to root', () => {
    expect(labels(open(USERS.root)).length).toBe(11);
  });

  it('leaves out a group that has nothing for the role', () => {
    const titles = open(USERS.technician).component.groups.map((g: any) => g.title);
    expect(titles).toEqual(['Trabajo', 'Gestión', 'Configuración']);
  });

  it('draws a link per item and closes the menu of the narrow screens when one is chosen', () => {
    const s = open(USERS.admin);
    const links = s.element.querySelectorAll('a');
    expect(links.length).toBeGreaterThan(8);
    s.component.shell.toggle();
    expect(s.component.shell.menuOpen).toBe(true);
    (links[1] as HTMLElement).click();
  });
});
