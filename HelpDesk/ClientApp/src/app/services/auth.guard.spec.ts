import { beforeEach, describe, expect, it, vi } from 'vitest';
import { AuthGuard } from './auth.guard';

describe('AuthGuard', () => {
  let user: any;
  let swal: any;
  let guard: AuthGuard;
  const levels = [{ name: 'ROOT', value: 4 }, { name: 'ADMINISTRADOR', value: 3 }, { name: 'MODERADOR', value: 2 }, { name: 'TECNICO', value: 1 }];
  const router: any = { parseUrl: (url: string) => ({ home: url }) };
  const route = (minLevel?: number): any => ({ data: minLevel === undefined ? {} : { minLevel } });

  beforeEach(() => {
    user = { acceso: 'TECNICO' };
    swal = vi.fn();
    const service: any = { getUser: () => user, levels, swal, messages: { titleAccessDenied: 'Acceso denegado', noPermissionToAccess: 'No tiene permisos' } };
    guard = new AuthGuard(service, router);
  });

  it('shows the login screen (blocks the route) when nobody is signed in', () => {
    user = null;
    expect(guard.canActivate(route(1))).toBe(false);
  });

  it('lets a technician into a screen of level 1', () => {
    expect(guard.canActivate(route(1))).toBe(true);
  });

  it('lets in a route that names no level', () => {
    expect(guard.canActivate(route())).toBe(true);
  });

  it('lets in a role above the minimum', () => {
    user = { acceso: 'ADMINISTRADOR' };
    expect(guard.canActivate(route(2))).toBe(true);
  });

  it('sends a technician home with a warning when the screen needs level 2', () => {
    const answer: any = guard.canActivate(route(2));
    expect(answer).toEqual({ home: '/' });
    expect(swal).toHaveBeenCalledWith('Acceso denegado', 'No tiene permisos', 'warning');
  });

  it('treats an unknown role as level 0', () => {
    user = { acceso: 'OTRO' };
    expect(guard.canActivate(route(1))).toEqual({ home: '/' });
  });
});
