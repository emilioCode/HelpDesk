import { afterEach, describe, expect, it, vi } from 'vitest';
import { CostumerComponent } from './costumer.component';
import { openScreen, USERS } from '../../../../testing/screen-testing';

const customers = [
  { id: 1, nombre: 'Farmacia San Rafael', rnc: '130000011', contacto: 'Ana', telefono: '809-555-0101', extension: '1', correo: 'f@x.test', departamento: 'Ventas', direccion: 'Calle 1', habilitado: true, idEmpresa: 1 },
  { id: 2, nombre: 'Colegio Horizonte', rnc: '130000022', contacto: 'Marta', telefono: '809-555-0122', extension: '210', correo: 'c@x.test', departamento: 'Informática', direccion: 'Av. 45', habilitado: true, idEmpresa: 1 }
];

function open(user = USERS.admin) {
  const s = openScreen(CostumerComponent, user);
  s.answer('Costumer/' + user.id + '/*', customers);
  s.detect();
  return s;
}

describe('CostumerComponent', () => {
  afterEach(() => vi.restoreAllMocks());

  it('lists the customers of the company', () => {
    const s = open();
    const rows = s.element.querySelectorAll('tbody tr');
    expect(rows.length).toBe(2);
    expect(s.element.textContent).toContain('Colegio Horizonte');
    expect(s.service.isLoading).toBe(false);
  });

  it('shows the message of the server when the list cannot be loaded', () => {
    const s = openScreen(CostumerComponent);
    s.detect();
    s.answer('Costumer/2/*', { errors: [{ detail: 'Sin acceso' }] }, { status: 400 });
    expect(s.swal).toHaveBeenCalledWith('Warning', 'Sin acceso', 'warning');
    expect(s.service.isLoading).toBe(false);
  });

  it('opens the real-time connection and closes it with the screen', () => {
    const s = open();
    expect(s.hub.start).toHaveBeenCalled();
    s.fixture.destroy();
    expect(s.hub.stop).toHaveBeenCalled();
  });

  describe('the window of a customer', () => {
    it('is empty and takes the company of the person when adding', () => {
      const s = open();
      s.component.fillModal('add', {});
      s.answer('Business/2', [{ id: 1, razonSocial: 'Demo Company' }]);
      expect(s.component.form.get('nombre')!.value).toBeFalsy();
      expect(s.component.costumer.idEmpresa).toBe(1);
      expect(s.component.form.valid).toBe(false);
    });

    it('shows the data of the customer when editing', () => {
      const s = open();
      s.component.fillModal('edit', { ...customers[1] });
      s.answer('Business/2', []);
      expect(s.component.form.get('nombre')!.value).toBe('Colegio Horizonte');
      expect(s.component.form.enabled).toBe(true);
    });

    it('is read only for a technician and for a window that disables', () => {
      const t = open(USERS.technician);
      t.component.fillModal('edit', { ...customers[0] });
      t.answer('Business/4', []);
      expect(t.component.form.disabled).toBe(true);
      const a = open();
      a.component.fillModal('delete', { ...customers[0] });
      a.answer('Business/2', []);
      expect(a.component.form.disabled).toBe(true);
    });

    it('lets only root choose the company', () => {
      const a = open();
      a.component.fillModal('edit', { ...customers[0] });
      a.answer('Business/2', []);
      expect(a.component.form.get('idEmpresa')!.disabled).toBe(true);
      const r = open(USERS.root);
      r.component.fillModal('edit', { ...customers[0] });
      r.answer('Business/1', []);
      expect(r.component.form.get('idEmpresa')!.disabled).toBe(false);
    });

    it('asks for the name and the RNC, and limits their length', () => {
      const s = open();
      s.component.fillModal('add', {}); s.answer('Business/2', []);
      const f = s.component.form;
      f.patchValue({ nombre: 'x'.repeat(101), rnc: 'y'.repeat(31) });
      expect(f.get('nombre')!.hasError('maxlength')).toBe(true);
      expect(f.get('rnc')!.hasError('maxlength')).toBe(true);
      f.patchValue({ nombre: 'Cliente', rnc: '1' });
      expect(f.valid).toBe(true);
    });
  });

  describe('saving', () => {
    const fill = (s: ReturnType<typeof open>) => {
      s.component.fillModal('add', {}); s.answer('Business/2', []);
      s.component.form.patchValue({ nombre: 'Cliente de prueba', rnc: '999' });
    };

    it('adds a customer with what was typed and the company, then tells the others', () => {
      const s = open(); fill(s);
      s.component.add();
      const req = s.http.expectOne(r => r.url.endsWith('api/Costumer') && r.method === 'POST');
      expect(req.request.body).toMatchObject({ nombre: 'Cliente de prueba', rnc: '999', idEmpresa: 1 });
      req.flush({ code: '1', title: 'Listo', message: 'Guardado', icon: 'success' });
      expect(s.toasts()).toContain('Listo: Guardado');
      expect(s.hub.invoke).toHaveBeenCalledWith('refresh', 'costumer', 1, 0, 0);
      expect(s.service.isLoading).toBe(false);
    });

    it('does not tell the others when the server refused', () => {
      const s = open(); fill(s);
      s.component.add();
      s.http.expectOne(r => r.method === 'POST').flush({ code: '0', title: 'No', message: 'Ya existe', icon: 'warning' });
      expect(s.hub.invoke).not.toHaveBeenCalled();
    });

    it('shows the error of the server when adding fails', () => {
      const s = open(); fill(s);
      s.component.add();
      s.http.expectOne(r => r.method === 'POST').flush({ errors: [{ detail: 'RNC repetido' }] }, { status: 400, statusText: 'Bad Request' });
      expect(s.swal).toHaveBeenCalledWith('Warning', 'RNC repetido', 'warning');
    });

    it('does not let a technician add or edit', () => {
      const s = open(USERS.technician);
      expect(s.component.add()).toBe(false);
      expect(s.component.edit()).toBe(false);
      expect(s.swal).toHaveBeenCalledTimes(2);
      s.http.expectNone(r => r.method === 'POST');
    });

    it('edits a customer and keeps its id', () => {
      const s = open();
      s.component.fillModal('edit', { ...customers[1] }); s.answer('Business/2', []);
      s.component.form.patchValue({ nombre: 'Colegio Horizonte Norte' });
      s.component.edit();
      const req = s.http.expectOne(r => r.url.endsWith('Costumer/Put/2'));
      expect(req.request.body).toMatchObject({ id: 2, nombre: 'Colegio Horizonte Norte', habilitado: true });
      req.flush({ code: '1', title: 'Listo', message: 'ok', icon: 'success' });
      expect(s.hub.invoke).toHaveBeenCalledWith('refresh', 'costumer', 1, 2, 0);
    });

    it('explains the validation errors of the server when editing fails', () => {
      const s = open();
      s.component.fillModal('edit', { ...customers[1] }); s.answer('Business/2', []);
      s.component.edit();
      s.http.expectOne(r => r.url.endsWith('Costumer/Put/2')).flush({ title: 'Datos no válidos', errors: { nombre: ['Falta el nombre'] } }, { status: 400, statusText: 'Bad Request' });
      expect(s.swal).toHaveBeenCalledWith('Datos no válidos', 'Falta el nombre', 'warning');
    });

    it('says the request failed when the connection is lost while editing', () => {
      const s = open();
      s.component.fillModal('edit', { ...customers[1] }); s.answer('Business/2', []);
      s.component.edit();
      s.http.expectOne(r => r.url.endsWith('Costumer/Put/2')).error(new ProgressEvent('error'));
      expect((s.swal.mock.calls[0] as any[])[1]).toContain('could not be completed');
      expect(s.service.isLoading).toBe(false);
    });

    it('disables and enables a customer by flipping its state', () => {
      const s = open();
      s.component.fillModal('delete', { ...customers[0] }); s.answer('Business/2', []);
      s.component.delete();
      const req = s.http.expectOne(r => r.url.endsWith('Costumer/Put/2'));
      expect(req.request.body.habilitado).toBe(false);
      req.flush({ code: '1', title: 'Listo', message: 'ok', icon: 'success' });
      expect(s.hub.invoke).toHaveBeenCalledWith('refresh', 'costumer', 1, 0, 0);
    });

    it('shows the error when disabling fails', () => {
      const s = open();
      s.component.fillModal('delete', { ...customers[0] }); s.answer('Business/2', []);
      s.component.delete();
      s.http.expectOne(r => r.url.endsWith('Costumer/Put/2')).flush({ errors: [{ detail: 'No se pudo' }] }, { status: 400, statusText: 'Bad Request' });
      expect(s.swal).toHaveBeenCalledWith('Warning', 'No se pudo', 'warning');
    });
  });

  describe('real-time notices', () => {
    it('reloads the list when another person changes the customers of the company', () => {
      const s = open();
      s.hub.emit('refresh', 'costumer', 1, 0, 0);
      s.answer('Costumer/2/*', [customers[0]]);
      expect(s.component.costumers.length).toBe(1);
    });

    it('ignores the notices of another company', () => {
      const s = open();
      s.hub.emit('refresh', 'costumer', 99, 0, 0);
      s.http.expectNone(r => r.url.includes('Costumer'));
    });

    it('puts what another person saved into the window that is open', () => {
      const s = open();
      s.component.fillModal('edit', { ...customers[1] }); s.answer('Business/2', []);
      s.hub.emit('refresh', 'costumer', 1, 2, 0);
      s.answer('Costumer/2/*', [{ ...customers[1], nombre: 'Colegio Horizonte Sur' }]);
      expect(s.component.form.get('nombre')!.value).toBe('Colegio Horizonte Sur');
    });

    it('shows the error when the reload fails', () => {
      const s = open();
      s.hub.emit('refresh', 'costumer', 1, 0, 0);
      s.answer('Costumer/2/*', { errors: [{ detail: 'Caído' }] }, { status: 400 });
      expect(s.swal).toHaveBeenCalled();
    });
  });
});
