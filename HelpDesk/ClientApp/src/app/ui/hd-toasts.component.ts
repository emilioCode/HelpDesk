import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { HdIconComponent } from './hd-icon.component';
import { ToastService } from './toast.service';

// Where the toasts appear. One per page, in the root of the app.
@Component({
  selector: 'hd-toasts',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [HdIconComponent],
  template: `
    <div class="hd-toasts" role="status" aria-live="polite">
      @for (t of toasts.toasts(); track t.id) {
        <div class="hd-toast">
          <hd-icon name="check" [size]="18"></hd-icon>
          <div class="hd-toast-text"><strong>{{ t.title }}</strong>@if (t.message) {<span>{{ t.message }}</span>}</div>
          <button type="button" class="hd-toast-close" aria-label="Cerrar aviso" (click)="toasts.dismiss(t.id)">&times;</button>
        </div>
      }
    </div>`
})
export class HdToastsComponent {
  toasts = inject(ToastService);
}
