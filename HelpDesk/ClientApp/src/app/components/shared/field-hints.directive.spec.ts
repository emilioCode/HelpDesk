import { describe, expect, it } from 'vitest';
import { Component } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TestBed } from '@angular/core/testing';
import { FieldHintsDirective } from './field-hints.directive';

@Component({
  standalone: true,
  imports: [ReactiveFormsModule, FieldHintsDirective],
  template: `
    <form [formGroup]="form">
      <div class="form-group" id="g1"><label>Nombre:</label><input formControlName="nombre"></div>
      <div class="form-group" id="g2"><label>Nota:</label><input formControlName="nota"></div>
      <div class="form-group" id="g3"><label>Clave:</label><input formControlName="clave" data-no-hints></div>
    </form>`
})
class Host {
  form = new FormGroup({
    nombre: new FormControl('', [Validators.required, Validators.maxLength(5)]),
    nota: new FormControl('', [Validators.minLength(3)]),
    clave: new FormControl('', [Validators.required])
  });
}

function setup() {
  const fixture = TestBed.createComponent(Host);
  fixture.detectChanges();
  const el = fixture.nativeElement as HTMLElement;
  const label = (id: string) => el.querySelector(`#${id} label`)!;
  const message = (id: string) => el.querySelector(`#${id} .field-error`);
  const type = (name: string, value: string) => {
    const control = fixture.componentInstance.form.get(name)!;
    control.setValue(value); control.markAsDirty();
    fixture.detectChanges();
  };
  return { fixture, el, label, message, type, form: fixture.componentInstance.form };
}

describe('FieldHintsDirective', () => {
  it('marks the label of a required field', () => {
    const { label } = setup();
    expect(label('g1').hasAttribute('data-required')).toBe(true);
  });

  it('does not mark a field that is not required', () => {
    const { label } = setup();
    expect(label('g2').hasAttribute('data-required')).toBe(false);
  });

  it('takes the mark off while the field is disabled', () => {
    const { label, form, fixture } = setup();
    form.get('nombre')!.disable(); fixture.detectChanges();
    expect(label('g1').hasAttribute('data-required')).toBe(false);
    form.get('nombre')!.enable(); fixture.detectChanges();
    expect(label('g1').hasAttribute('data-required')).toBe(true);
  });

  it('says nothing before the person touches the field', () => {
    const { message } = setup();
    expect(message('g1')).toBeNull();
  });

  it('says why a field is empty once it was changed', () => {
    const { message, type } = setup();
    type('nombre', 'a'); type('nombre', '');
    expect(message('g1')!.textContent).toBe('Este campo es obligatorio');
  });

  it('says how long a text may be, and how long it is', () => {
    const { message, type } = setup();
    type('nombre', 'abcdefg');
    expect(message('g1')!.textContent).toBe('Máximo 5 caracteres (lleva 7)');
  });

  it('says the minimum length', () => {
    const { message, type } = setup();
    type('nota', 'ab');
    expect(message('g2')!.textContent).toBe('Mínimo 3 caracteres');
  });

  it('removes the message when the value becomes valid, and does not repeat it', () => {
    const { el, type } = setup();
    type('nombre', 'abcdefg'); type('nombre', 'abcdefg');
    expect(el.querySelectorAll('#g1 .field-error').length).toBe(1);
    type('nombre', 'abc');
    expect(el.querySelectorAll('#g1 .field-error').length).toBe(0);
  });

  it('leaves alone a field that opts out', () => {
    const { label, message, type } = setup();
    type('clave', '');
    expect(label('g3').hasAttribute('data-required')).toBe(false);
    expect(message('g3')).toBeNull();
  });

  it('describes the first rule that fails', () => {
    expect(FieldHintsDirective.describe(new FormControl('', [Validators.required]))).toBe('Este campo es obligatorio');
    expect(FieldHintsDirective.describe(new FormControl(2, [Validators.min(5)]))).toBe('Debe ser al menos 5');
    expect(FieldHintsDirective.describe(new FormControl('x', [Validators.email]))).toBe('Valor no válido');
  });
});
