import { Component, DoCheck, EventEmitter, Input, Output } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ApiService } from '../../../../services/api.service';
import { MirroredForm } from './mirrored-form';

// The parts used in a ticket, with the row to add one and, when the screen allows it, the buttons to edit or remove one.
@Component({
  selector: 'app-ticket-parts',
  templateUrl: './ticket-parts.component.html',
  styles: [':host { display: block; }']
})
export class TicketPartsComponent implements DoCheck {
  @Input() ticket: any = {};
  @Input() parts: any = [];
  // The part being written in the adding row.
  @Input() part: any = {};
  @Input() option: string;
  @Input() addPart = false;
  // False on the screens where the parts can only be read (no edit or remove buttons).
  @Input() actions = true;
  // The tooltip of the remove button. The ticket screen has always said "editar" here; it is kept as it was.
  @Input() deleteTitle = 'eliminar';
  @Output() startAdd = new EventEmitter<void>();
  @Output() save = new EventEmitter<any>();
  @Output() cancel = new EventEmitter<void>();
  // The person chose a part to edit or to remove.
  @Output() choose = new EventEmitter<any>();

  // The adding row: quantity, serial (up to 50, as the server) and description, kept equal to the part the screen gives.
  form: MirroredForm;

  constructor(public service: ApiService, formBuilder: FormBuilder) {
    this.form = new MirroredForm(formBuilder.group({
      cantidad: [null, [Validators.required, Validators.min(1)]],
      noSerial: [null, [Validators.required, Validators.maxLength(50)]],
      descripcion: [null, Validators.required]
    }), () => this.part);
  }

  ngDoCheck() {
    this.form.sync();
  }

  // The screen checks what is required; a serial that is too long is stopped here, before asking the server.
  onSave() {
    if (this.form.group.get('noSerial').hasError('maxlength')) {
      this.service.swal(this.service.messages.titleRequiredFields, this.service.messages.partSerialTooLong, 'warning');
      return;
    }
    this.save.emit(this.part);
  }
}
