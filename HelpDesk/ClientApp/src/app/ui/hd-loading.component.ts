import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { HdSpinnerComponent } from './hd-spinner.component';

// A veil with a spinner over the card or window it sits in (the parent must be position: relative, as .box and .modal-content are).
//   <hd-loading [active]="service.isLoading"></hd-loading>
@Component({
  selector: 'hd-loading',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [HdSpinnerComponent],
  host: { style: 'display: contents' },
  template: `<div class="hd-loading" [hidden]="!active()"><hd-spinner size="lg"></hd-spinner></div>`
})
export class HdLoadingComponent {
  active = input(false);
}
