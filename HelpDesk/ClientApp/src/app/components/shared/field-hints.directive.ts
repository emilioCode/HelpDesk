import { Directive, DoCheck, ElementRef, OnDestroy, Renderer2 } from '@angular/core';
import { AbstractControl, FormControl, NgControl } from '@angular/forms';

// The marks every reactive form shows, written once. It sits on every control of a form (formControlName) and:
//  - marks the label of the field (in its ".form-group") with the attribute data-required while the control is required and editable,
//    so the label gets its "*" from the stylesheet (styles.css), which is the only place that says how it looks;
//  - shows, under the control, why its value is not valid, once the person has touched or changed it.
// A control opts out with the attribute data-no-hints (the sign-in screen has no labels and says nothing until you submit).
@Directive({
  standalone: false,
  selector: '[formControlName]:not([data-no-hints])'
})
export class FieldHintsDirective implements DoCheck, OnDestroy {
  private message: HTMLElement = null;
  private shown = '';

  constructor(private control: NgControl, private element: ElementRef, private renderer: Renderer2) { }

  ngDoCheck() {
    const control = this.control.control;
    if (!control) { return; }
    this.markRequired(control);
    this.showMessage(control);
  }

  ngOnDestroy() {
    this.removeMessage();
  }

  private markRequired(control: AbstractControl) {
    const failsWhenEmpty = control.validator ? control.validator(new FormControl('')) : null;
    const required = !!(failsWhenEmpty && failsWhenEmpty.required) && control.enabled;
    // The mark is on the label of the group (which nothing else rewrites) and is checked on every pass, so it cannot be lost.
    const group = (this.element.nativeElement as HTMLElement).closest('.form-group');
    const label = group ? group.querySelector(':scope > label') : null;
    if (!label || label.hasAttribute('data-required') === required) { return; }
    if (required) { this.renderer.setAttribute(label, 'data-required', ''); } else { this.renderer.removeAttribute(label, 'data-required'); }
  }

  private showMessage(control: AbstractControl) {
    const text = control.enabled && control.invalid && (control.touched || control.dirty) ? FieldHintsDirective.describe(control) : '';
    if (text === this.shown) { return; }
    this.shown = text;
    if (!text) {
      this.removeMessage();
      return;
    }
    if (!this.message) {
      const host = this.element.nativeElement as HTMLElement;
      this.message = this.renderer.createElement('small');
      this.renderer.addClass(this.message, 'field-error');
      this.renderer.insertBefore(host.parentNode, this.message, host.nextSibling);
    }
    this.message.textContent = text;
  }

  private removeMessage() {
    if (this.message && this.message.parentNode) {
      this.renderer.removeChild(this.message.parentNode, this.message);
    }
    this.message = null;
  }

  // The first rule that fails, in words.
  static describe(control: AbstractControl): string {
    const errors = control.errors || {};
    if (errors.required) { return 'Este campo es obligatorio'; }
    if (errors.maxlength) { return `Máximo ${errors.maxlength.requiredLength} caracteres (lleva ${errors.maxlength.actualLength})`; }
    if (errors.minlength) { return `Mínimo ${errors.minlength.requiredLength} caracteres`; }
    if (errors.min) { return `Debe ser al menos ${errors.min.min}`; }
    return 'Valor no válido';
  }
}
