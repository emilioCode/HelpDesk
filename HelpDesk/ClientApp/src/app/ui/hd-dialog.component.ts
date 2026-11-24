import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

// A window that opens over the page. It uses the markup the page behaviors (page-behaviors.ts) already drive, so any
// element with data-toggle="modal" data-target="#<id>" opens it, and the close buttons (data-dismiss="modal") and Escape close it.
//   <hd-dialog dialogId="terms" title="Términos y condiciones" size="lg"> ...body... </hd-dialog>
@Component({
  selector: 'hd-dialog',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { style: 'display: contents' },
  template: `
    <div class="modal fade" [id]="dialogId()" tabindex="-1" role="dialog" aria-hidden="true" [attr.data-backdrop]="staticBackdrop() ? 'static' : null">
      <div class="modal-dialog" [class.modal-lg]="size() === 'lg'" role="document">
        <div class="modal-content">
          <div class="modal-header">
            <button type="button" class="close" data-dismiss="modal" aria-label="Cerrar"><span aria-hidden="true">&times;</span></button>
            <h4 class="modal-title">{{ title() }}</h4>
          </div>
          <div class="modal-body"><ng-content></ng-content></div>
          <div class="modal-footer">
            <button type="button" class="hd-btn hd-btn-secondary" data-dismiss="modal">{{ closeLabel() }}</button>
          </div>
        </div>
      </div>
    </div>`
})
export class HdDialogComponent {
  // The element id the opener points at. "id" would also set it on the host, so the name is dialogId.
  dialogId = input.required<string>();
  title = input('');
  size = input<'md' | 'lg'>('md');
  staticBackdrop = input(false);
  closeLabel = input('Cerrar');
}
