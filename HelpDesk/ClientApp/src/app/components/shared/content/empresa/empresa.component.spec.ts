import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { EmpresaComponent } from './empresa.component';
import { openScreen, USERS } from '../../../../testing/screen-testing';

const companies = [
  { id: 1, razonSocial: 'Demo Company', sectorComercial: 'Servicios', rnc: '101', telefono: '809', noAutorizacion: '', correo: 's@demo.test', contrasena: null, secuenciaticket: '13', url: '', host: 'mailpit', port: 1025, direccion: 'Av. 1', condicionesTaller: '', condicionesDomicilio: '', habilitado: true, image: null }
];

function open(user: any = USERS.admin) {
  const s = openScreen(EmpresaComponent, user);
  if (user.acceso === 'ADMINISTRADOR' || user.acceso === 'ROOT') s.answer('Business/' + user.id, companies);
  s.detect();
  return s;
}

describe('EmpresaComponent', () => {
  const alertSpy = vi.fn();
  beforeEach(() => { alertSpy.mockClear(); vi.stubGlobal('alert', alertSpy); });
  afterEach(() => { vi.restoreAllMocks(); vi.unstubAllGlobals(); });

  it('lists the companies', () => {
    const s = open();
    expect(s.element.querySelectorAll('tbody tr').length).toBe(1);
    expect(s.element.textContent).toContain('Demo Company');
  });

  it('keeps out a person below administrator', () => {
    const s = openScreen(EmpresaComponent, USERS.moderator);
    s.detect();
    expect(alertSpy).toHaveBeenCalledWith(s.service.messages.noPermissionToAccess);
    s.http.expectNone(r => r.url.includes('/Business/'));
  });

  it('shows the error when the list cannot be loaded', () => {
    const s = openScreen(EmpresaComponent);
    s.detect();
    s.answer('Business/2', { errors: [{ detail: 'Caído' }] }, { status: 400 });
    expect(s.swal).toHaveBeenCalledWith('Warning', 'Caído', 'warning');
  });

  describe('the window of a company', () => {
    it('shows the data when editing and asks for the name and the sequence of orders', () => {
      const s = open();
      s.component.fillModal('edit', { ...companies[0] });
      expect(s.component.form.get('razonSocial')!.value).toBe('Demo Company');
      s.component.form.patchValue({ razonSocial: '', secuenciaticket: '' });
      expect(s.component.form.get('razonSocial')!.hasError('required')).toBe(true);
      expect(s.component.form.get('secuenciaticket')!.hasError('required')).toBe(true);
    });

    it('limits the length of the fields and checks the e-mail', () => {
      const s = open();
      s.component.fillModal('edit', { ...companies[0] });
      s.component.form.patchValue({ noAutorizacion: 'x'.repeat(13), correo: 'no', direccion: 'd'.repeat(151) });
      expect(s.component.form.get('noAutorizacion')!.hasError('maxlength')).toBe(true);
      expect(s.component.form.get('correo')!.hasError('email')).toBe(true);
      expect(s.component.form.get('direccion')!.hasError('maxlength')).toBe(true);
    });

    it('disables the form when the window asks to disable', () => {
      const s = open();
      s.component.fillModal('delete', { ...companies[0] });
      expect(s.component.form.disabled).toBe(true);
    });

    it('folds the open notes panels when it opens', () => {
      const s = open();
      const panel = s.element.querySelector('#collapseOne')!; panel.classList.add('in');
      const btn = s.element.querySelector('#collapseOnebtn') as HTMLElement; const clicked = vi.fn(); btn.addEventListener('click', clicked);
      s.component.fillModal('edit', { ...companies[0] });
      expect(clicked).toHaveBeenCalled();
    });
  });

  describe('the logo', () => {
    it('keeps the picture that was chosen, as text of base64', async () => {
      const s = open();
      s.component.fillModal('edit', { ...companies[0] });
      s.component.handleFileInput({ item: () => new File(['abc'], 'l.png', { type: 'image/png' }) } as any);
      await vi.waitFor(() => expect(s.component.empresa.image).toBe('YWJj'));
    });
  });

  describe('saving', () => {
    const ready = (option: 'add' | 'edit' | 'delete') => {
      const s = open();
      s.component.fillModal(option, option === 'add' ? {} : { ...companies[0] });
      s.detect();
      return s;
    };

    it('adds a company, reloads the list and closes the window', () => {
      const s = ready('add');
      s.component.form.patchValue({ razonSocial: 'Nueva', secuenciaticket: '1' });
      const closed = vi.fn(); s.element.querySelector('#closeModalBtn')!.addEventListener('click', closed);
      s.component.add();
      const req = s.http.expectOne(r => r.url.endsWith('api/Business/2') && r.method === 'POST');
      expect(req.request.body).toMatchObject({ razonSocial: 'Nueva' });
      req.flush({ code: '1', title: 'Listo', message: 'ok', icon: 'success' });
      // the list is asked again, and the close button of the window asks for it once more
      expect(s.http.match(r => r.method === 'GET' && r.url.endsWith('Business/2')).length).toBe(2);
      expect(closed).toHaveBeenCalled();
    });

    it('explains why adding failed', () => {
      const s = ready('add');
      s.component.add();
      s.http.expectOne(r => r.method === 'POST').flush({ title: 'Datos no válidos', errors: { razonSocial: ['Falta'] } }, { status: 400, statusText: 'Bad Request' });
      expect(s.swal).toHaveBeenCalledWith('Datos no válidos', 'Falta', 'warning');
    });

    it('edits a company and tells the others', () => {
      const s = ready('edit');
      s.component.form.patchValue({ razonSocial: 'Demo Company SRL' });
      s.component.edit();
      const req = s.http.expectOne(r => r.url.endsWith('Business/Put/2'));
      expect(req.request.body).toMatchObject({ id: 1, razonSocial: 'Demo Company SRL' });
      req.flush({ code: '1', title: 'Listo', message: 'ok', icon: 'success' });
      expect(s.hub.invoke).toHaveBeenCalledWith('refresh', 'empresa', 1, 0, 0);
    });

    it('says the request failed when the connection is lost while editing', () => {
      const s = ready('edit');
      s.component.edit();
      s.http.expectOne(r => r.url.endsWith('Business/Put/2')).error(new ProgressEvent('error'));
      expect((s.swal.mock.calls[0] as any[])[1]).toContain('could not be completed');
    });

    it('disables a company and tells the others', () => {
      const s = ready('delete');
      s.component.delete();
      const req = s.http.expectOne(r => r.url.endsWith('Business/Put/2'));
      req.flush({ code: '1', title: 'Listo', message: 'ok', icon: 'success' });
      expect(s.hub.invoke).toHaveBeenCalledWith('refresh', 'empresa', 1, 0, 0);
    });

    it('shows the error when disabling fails', () => {
      const s = ready('delete');
      s.component.delete();
      s.http.expectOne(r => r.url.endsWith('Business/Put/2')).flush({ errors: [{ detail: 'No' }] }, { status: 400, statusText: 'Bad Request' });
      expect(s.swal).toHaveBeenCalledWith('Warning', 'No', 'warning');
    });
  });

  describe('real-time notices', () => {
    it('reloads the companies and shows the one of the person when it changes', () => {
      const s = open();
      s.hub.emit('refresh', 'empresa', 1, 0, 0);
      s.answer('Business/2', [{ ...companies[0], razonSocial: 'Cambiada' }]);
      expect(s.component.form.get('razonSocial')!.value).toBe('Cambiada');
    });

    it('ends the session when the company was disabled', () => {
      const s = open();
      s.hub.emit('refresh', 'empresa', 1, 0, 0);
      s.answer('Business/2', [{ ...companies[0], habilitado: false }]);
      s.http.expectOne(r => r.url.endsWith('Login/Logout'));
    });

    it('ends the session when the person was disabled', () => {
      const s = open();
      s.hub.emit('refresh', 'session', 1, 2, 0);
      expect(alertSpy).toHaveBeenCalledWith(s.service.messages.userDisabled);
      s.http.expectOne(r => r.url.endsWith('Login/Logout'));
    });

    it('ignores the notices of other companies', () => {
      const s = open();
      s.hub.emit('refresh', 'empresa', 9, 0, 0);
      s.http.expectNone(r => r.url.includes('/Business/'));
    });

    it('shows the error when the reload fails', () => {
      const s = open();
      s.hub.emit('refresh', 'empresa', 1, 0, 0);
      s.answer('Business/2', { errors: [{ detail: 'Caído' }] }, { status: 400 });
      expect(s.swal).toHaveBeenCalled();
    });
  });

  it('closes the real-time connection with the screen', () => {
    const s = open();
    s.fixture.destroy();
    expect(s.hub.stop).toHaveBeenCalled();
  });
});
