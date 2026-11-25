import { beforeEach, describe, expect, it } from 'vitest';
import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { HdAlertComponent } from './hd-alert.component';
import { HdBadgeComponent } from './hd-badge.component';
import { HdButtonDirective } from './hd-button.directive';
import { HdDialogComponent } from './hd-dialog.component';
import { HdEmptyComponent } from './hd-empty.component';
import { HdLoadingComponent } from './hd-loading.component';
import { HdProgressComponent } from './hd-progress.component';
import { HdSpinnerComponent } from './hd-spinner.component';

// A screen that imports every piece; each test gives it the template it wants to try.
@Component({ selector: 'test-host', template: '', imports: [HdAlertComponent, HdBadgeComponent, HdButtonDirective, HdDialogComponent, HdEmptyComponent, HdLoadingComponent, HdProgressComponent, HdSpinnerComponent] })
class Host { }

function render(_piece: unknown, _inputs: Record<string, unknown>, content: string) {
  TestBed.overrideComponent(Host, { set: { template: content } });
  const fixture = TestBed.createComponent(Host);
  fixture.detectChanges();
  return fixture;
}

function standalone(type: any, inputs: Record<string, unknown>): { root: HTMLElement, update: (v: Record<string, unknown>) => void } {
  TestBed.configureTestingModule({});
  const fixture = TestBed.createComponent(type);
  Object.entries(inputs).forEach(([k, v]) => fixture.componentRef.setInput(k, v));
  fixture.detectChanges();
  return { root: fixture.nativeElement as HTMLElement, update: (v) => { Object.entries(v).forEach(([k, x]) => fixture.componentRef.setInput(k, x)); fixture.detectChanges(); } };
}

describe('base components', () => {
  beforeEach(() => TestBed.configureTestingModule({}));

  describe('hd-badge', () => {
    it('is a pill tinted by the state', () => {
      const { root } = standalone(HdBadgeComponent, { tone: 'prog' });
      const pill = root.querySelector('.hd-badge')!;
      expect(pill.classList.contains('hd-badge-prog')).toBe(true);
    });
    it('changes tone and size', () => {
      const { root, update } = standalone(HdBadgeComponent, { tone: 'open' });
      update({ tone: 'appr', small: true });
      const pill = root.querySelector('.hd-badge')!;
      expect(pill.classList.contains('hd-badge-appr')).toBe(true);
      expect(pill.classList.contains('hd-badge-open')).toBe(false);
      expect(pill.classList.contains('hd-badge-sm')).toBe(true);
    });
    it('shows the text it is given', () => {
      const f = render(HdBadgeComponent, {}, '<hd-badge tone="done">Completado</hd-badge>');
      expect(f.nativeElement.querySelector('.hd-badge').textContent.trim()).toBe('Completado');
    });
  });

  describe('hd-progress', () => {
    const bar = (root: HTMLElement) => root.querySelector('[role="progressbar"]') as HTMLElement;
    it('tells screen readers where it stands', () => {
      const { root } = standalone(HdProgressComponent, { value: 50, label: 'Progreso de la orden' });
      expect(bar(root).getAttribute('aria-valuenow')).toBe('50');
      expect(bar(root).getAttribute('aria-label')).toBe('Progreso de la orden');
      expect(root.textContent).toContain('50%');
      expect((root.querySelector('.hd-progress-fill') as HTMLElement).style.width).toBe('50%');
    });
    it('never goes under 0 or over 100, and rounds', () => {
      const { root, update } = standalone(HdProgressComponent, { value: 140 });
      expect(bar(root).getAttribute('aria-valuenow')).toBe('100');
      update({ value: -5 }); expect(bar(root).getAttribute('aria-valuenow')).toBe('0');
      update({ value: 33.6 }); expect(bar(root).getAttribute('aria-valuenow')).toBe('34');
    });
    it('treats a value that is not a number as 0', () => {
      const { root } = standalone(HdProgressComponent, { value: 'abc' });
      expect(bar(root).getAttribute('aria-valuenow')).toBe('0');
    });
    it('paints the finished tone green', () => {
      const { root } = standalone(HdProgressComponent, { value: 100, tone: 'done' });
      expect(root.querySelector('.hd-progress-fill')!.classList.contains('hd-progress-done')).toBe(true);
    });
  });

  describe('hd-spinner', () => {
    it('announces itself as a status with a label', () => {
      const { root } = standalone(HdSpinnerComponent, { label: 'Cargando su orden', size: 'lg' });
      const ring = root.querySelector('.hd-spin')!;
      expect(ring.getAttribute('role')).toBe('status');
      expect(ring.getAttribute('aria-label')).toBe('Cargando su orden');
      expect(ring.classList.contains('hd-spin-lg')).toBe(true);
    });
  });

  describe('hd-loading', () => {
    it('is hidden until it is active', () => {
      const { root, update } = standalone(HdLoadingComponent, {});
      const veil = root.querySelector('.hd-loading') as HTMLElement;
      expect(veil.hidden).toBe(true);
      update({ active: true });
      expect(veil.hidden).toBe(false);
      expect(veil.querySelector('.hd-spin')).not.toBeNull();
    });
  });

  describe('hd-alert', () => {
    it('is a note for information, and an alert for a warning or an error', () => {
      const { root, update } = standalone(HdAlertComponent, { tone: 'info' });
      const box = () => root.querySelector('.hd-alert')!;
      expect(box().classList.contains('hd-alert-info')).toBe(true);
      expect(box().getAttribute('role')).toBe('note');
      update({ tone: 'warn' }); expect(box().getAttribute('role')).toBe('alert');
      update({ tone: 'error' }); expect(box().getAttribute('role')).toBe('alert');
      update({ tone: 'success' }); expect(box().getAttribute('role')).toBe('note');
    });
  });

  describe('hd-empty', () => {
    it('shows the message it is given', () => {
      const f = render(HdEmptyComponent, {}, '<hd-empty>No hay órdenes que mostrar.</hd-empty>');
      expect(f.nativeElement.querySelector('.hd-empty').textContent.trim()).toBe('No hay órdenes que mostrar.');
    });
  });

  describe('hdButton', () => {
    const button = (attrs: string) => render(HdButtonDirective, {}, `<button hdButton ${attrs}>Ok</button>`).nativeElement.querySelector('button') as HTMLElement;
    it('is secondary by default', () => {
      const b = button('');
      expect(b.classList.contains('hd-btn')).toBe(true);
      expect(b.classList.contains('hd-btn-secondary')).toBe(true);
    });
    it('takes the variant, the size and the full width', () => {
      const b = button('variant="primary" size="lg" [block]="true"');
      expect(b.classList.contains('hd-btn-primary')).toBe(true);
      expect(b.classList.contains('hd-btn-lg')).toBe(true);
      expect(b.classList.contains('hd-btn-block')).toBe(true);
    });
    it('adds no size class for the normal size', () => {
      const b = button('variant="danger"');
      expect([...b.classList].some(c => c === 'hd-btn-md')).toBe(false);
    });
    it('keeps the classes the screen gives it', () => {
      const b = button('variant="primary" class="hd-create"');
      expect(b.classList.contains('hd-create')).toBe(true);
      expect(b.classList.contains('hd-btn-primary')).toBe(true);
    });
  });

  describe('hd-dialog', () => {
    const dialog = (attrs: string, body = 'texto') => render(HdDialogComponent, {}, `<hd-dialog ${attrs}>${body}</hd-dialog>`).nativeElement as HTMLElement;
    it('carries the id the opener points at, on the window itself', () => {
      const root = dialog('dialogId="terms" title="Términos"');
      expect(root.querySelector('.modal')!.id).toBe('terms');
    });
    it('shows the title, the body and a close button wired to data-dismiss', () => {
      const root = dialog('dialogId="t" title="Términos y condiciones"', '<p id="b">hola</p>');
      expect(root.querySelector('.modal-title')!.textContent).toBe('Términos y condiciones');
      expect(root.querySelector('.modal-body #b')).not.toBeNull();
      expect(root.querySelectorAll('[data-dismiss="modal"]').length).toBe(2);
      expect(root.querySelector('.modal-footer .hd-btn')!.textContent!.trim()).toBe('Cerrar');
    });
    it('can name its close button', () => {
      const root = dialog('dialogId="t" closeLabel="Ok"');
      expect(root.querySelector('.modal-footer .hd-btn')!.textContent!.trim()).toBe('Ok');
    });
    it('is wide when asked, and static when asked', () => {
      const root = dialog('dialogId="t" size="lg" [staticBackdrop]="true"');
      expect(root.querySelector('.modal-dialog')!.classList.contains('modal-lg')).toBe(true);
      expect(root.querySelector('.modal')!.getAttribute('data-backdrop')).toBe('static');
    });
    it('does not set data-backdrop when it is not static', () => {
      const root = dialog('dialogId="t"');
      expect(root.querySelector('.modal')!.hasAttribute('data-backdrop')).toBe(false);
    });
  });
});
