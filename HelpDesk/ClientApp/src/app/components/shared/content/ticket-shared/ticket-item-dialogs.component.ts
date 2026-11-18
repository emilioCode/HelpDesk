import { Component, DoCheck, EventEmitter, Input, Output, ChangeDetectionStrategy } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ApiService } from '../../../../services/api.service';
import { MirroredForm } from './mirrored-form';

// The four dialogs of a ticket to edit or remove a part used or a device. They are opened from the buttons of the
// lists (bootstrap, by id), work on the part or device the screen gives them and tell the screen what was decided.
@Component({
  standalone: false,
  selector: 'app-ticket-item-dialogs',
  templateUrl: './ticket-item-dialogs.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styles: [':host { display: block; }']
})
export class TicketItemDialogsComponent implements DoCheck {
  @Input() parToEdit: any = {};
  @Input() deviceToEdit: any = {};
  @Output() editPart = new EventEmitter<any>();
  @Output() deletePart = new EventEmitter<any>();
  @Output() closeParts = new EventEmitter<void>();
  @Output() editDevice = new EventEmitter<any>();
  @Output() deleteDevice = new EventEmitter<any>();
  @Output() closeDevices = new EventEmitter<void>();

  // Each dialog has its own form over the part or device the screen gives. The ones to edit take what is typed; the ones to remove
  // only show it (all their controls are locked). Limits are the ones of the server (PieceValidator, DeviceValidator).
  partEdit: MirroredForm;
  partRemove: MirroredForm;
  deviceEdit: MirroredForm;
  deviceRemove: MirroredForm;

  constructor(public service: ApiService, private formBuilder: FormBuilder) {
    const part = () => this.formBuilder.group({
      cantidad: [null, [Validators.required, Validators.min(1)]],
      noSerial: [null, [Validators.required, Validators.maxLength(50)]],
      descripcion: [null, Validators.required]
    });
    const device = () => this.formBuilder.group({
      descripcion: [null, Validators.required],
      fallaReportada: [null, [Validators.required, Validators.maxLength(80)]],
      marca: [null, [Validators.required, Validators.maxLength(50)]],
      modelo: [null, [Validators.required, Validators.maxLength(50)]],
      noSerial: [null, [Validators.required, Validators.maxLength(50)]]
    });
    this.partEdit = new MirroredForm(part(), () => this.parToEdit);
    this.partRemove = new MirroredForm(part(), () => this.parToEdit);
    this.deviceEdit = new MirroredForm(device(), () => this.deviceToEdit);
    this.deviceRemove = new MirroredForm(device(), () => this.deviceToEdit);
    this.partRemove.setAllDisabled(true);
    this.deviceRemove.setAllDisabled(true);
  }

  ngDoCheck() {
    [this.partEdit, this.partRemove, this.deviceEdit, this.deviceRemove].forEach(f => f.sync());
  }
}
