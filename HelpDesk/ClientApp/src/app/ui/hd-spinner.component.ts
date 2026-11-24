import { ChangeDetectionStrategy, Component, input } from '@angular/core';

// A small turning ring. Respects "reduce motion".
//   <hd-spinner size="lg" label="Cargando"></hd-spinner>
@Component({
  selector: 'hd-spinner',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { style: 'display: contents' },
  template: `<span class="hd-spin" [class.hd-spin-lg]="size() === 'lg'" role="status" [attr.aria-label]="label()"></span>`
})
export class HdSpinnerComponent {
  size = input<'md' | 'lg'>('md');
  label = input('Cargando');
}
