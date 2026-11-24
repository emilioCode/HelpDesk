import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

// A bar with its label and percentage above it.
//   <hd-progress [value]="50" label="Progreso de la orden"></hd-progress>
// tone "done" paints the bar in the green of a finished order.
@Component({
  selector: 'hd-progress',
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="hd-progress-row"><span>{{ label() }}</span><span>{{ shown() }}%</span></div>
    <div class="hd-progress" role="progressbar" [attr.aria-label]="label()" aria-valuemin="0" aria-valuemax="100" [attr.aria-valuenow]="shown()">
      <div class="hd-progress-fill" [class.hd-progress-done]="tone() === 'done'" [style.width.%]="shown()"></div>
    </div>`,
  styles: [':host { display: flex; flex-direction: column; gap: 8px; }']
})
export class HdProgressComponent {
  value = input(0);
  label = input('Progreso');
  tone = input<'accent' | 'done'>('accent');
  shown = computed(() => Math.max(0, Math.min(100, Math.round(Number(this.value()) || 0))));
}
