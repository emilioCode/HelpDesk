import { AbstractControl, FormGroup, ValidationErrors } from '@angular/forms';

// A reactive form that works on a plain object the screen already reads and changes everywhere.
//  - What the person types in a control goes to the object right away (before the code of the change event runs).
//  - sync() puts what the code left in the object back in the controls; call it on every check (ngDoCheck).
export class MirroredForm {
  constructor(public readonly group: FormGroup, private model: () => any) {
    Object.keys(group.controls).forEach(name => {
      group.get(name).valueChanges.subscribe(value => {
        const target = this.model();
        if (target) { target[name] = value; }
      });
    });
  }

  sync() {
    const source = this.model();
    Object.keys(this.group.controls).forEach(name => {
      const control = this.group.get(name);
      const value = source && source[name] !== undefined ? source[name] : null;
      if (control.value !== value) {
        control.setValue(value, { emitEvent: false });
      }
    });
  }

  // Enables or disables a control without telling anybody (the state follows the conditions of the screen).
  setDisabled(name: string, disabled: boolean) {
    const control = this.group.get(name);
    if (disabled && control.enabled) {
      control.disable({ emitEvent: false });
    } else if (!disabled && control.disabled) {
      control.enable({ emitEvent: false });
    }
  }

  setAllDisabled(disabled: boolean) {
    Object.keys(this.group.controls).forEach(name => this.setDisabled(name, disabled));
  }
}

// A value made only of spaces counts as empty (what the old code checked with validateTrim).
export function notBlank(control: AbstractControl): ValidationErrors | null {
  const value = control.value;
  return value == null || String(value).trim() === '' ? { required: true } : null;
}
