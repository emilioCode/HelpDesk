import { ChangeDetectionStrategy, Component, input } from '@angular/core';

export type HdTone = 'open' | 'prog' | 'done' | 'appr';

// The status pill: a dot and a short word, tinted by the state of an order.
//   <hd-badge tone="prog">En proceso</hd-badge>
@Component({
  selector: 'hd-badge',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { style: 'display: contents' },
  template: `<span class="hd-badge" [class]="'hd-badge hd-badge-' + tone() + (small() ? ' hd-badge-sm' : '')"><ng-content></ng-content></span>`
})
export class HdBadgeComponent {
  tone = input<HdTone>('open');
  small = input(false);
}
