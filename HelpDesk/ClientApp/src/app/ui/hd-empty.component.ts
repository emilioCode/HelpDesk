import { ChangeDetectionStrategy, Component } from '@angular/core';

// What a list shows when there is nothing to list.
//   <hd-empty>No hay órdenes que mostrar.</hd-empty>
@Component({
  selector: 'hd-empty',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { style: 'display: contents' },
  template: `<p class="hd-empty"><ng-content></ng-content></p>`
})
export class HdEmptyComponent { }
