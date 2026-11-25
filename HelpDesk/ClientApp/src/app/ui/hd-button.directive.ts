import { Directive, computed, input } from '@angular/core';

// Gives a <button> or an <a> the look of the design.
//   <button hdButton variant="primary" size="lg" [block]="true">Entrar</button>
//   <a hdButton variant="secondary" size="sm" href="...">Imprimir</a>
@Directive({
  selector: '[hdButton]',
  host: { '[class]': 'classes()' }
})
export class HdButtonDirective {
  variant = input<'primary' | 'secondary' | 'ghost' | 'danger'>('secondary');
  size = input<'sm' | 'md' | 'lg'>('md');
  block = input(false);
  classes = computed(() => 'hd-btn hd-btn-' + this.variant() + (this.size() !== 'md' ? ' hd-btn-' + this.size() : '') + (this.block() ? ' hd-btn-block' : ''));
}
