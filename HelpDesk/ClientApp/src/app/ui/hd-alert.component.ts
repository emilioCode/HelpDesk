import { ChangeDetectionStrategy, Component, input } from '@angular/core';

// A message inside a page: a note, a warning, an error or a confirmation.
//   <hd-alert tone="info"><strong>Nota:</strong> texto</hd-alert>
@Component({
  selector: 'hd-alert',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { style: 'display: contents' },
  template: `<div [class]="'hd-alert hd-alert-' + tone()" [attr.role]="tone() === 'error' || tone() === 'warn' ? 'alert' : 'note'"><ng-content></ng-content></div>`
})
export class HdAlertComponent {
  tone = input<'info' | 'warn' | 'error' | 'success'>('info');
}
