import { Component, EventEmitter, Input, Output } from '@angular/core';
import { ApiService } from '../../../../services/api.service';

// The parts used in a ticket, with the row to add one and, when the screen allows it, the buttons to edit or remove one.
@Component({
  selector: 'app-ticket-parts',
  templateUrl: './ticket-parts.component.html',
  styles: [':host { display: block; }']
})
export class TicketPartsComponent {
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

  constructor(public service: ApiService) { }
}
