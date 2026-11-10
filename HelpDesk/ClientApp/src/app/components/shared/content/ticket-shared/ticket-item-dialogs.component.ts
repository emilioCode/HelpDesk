import { Component, EventEmitter, Input, Output } from '@angular/core';
import { ApiService } from '../../../../services/api.service';

// The four dialogs of a ticket to edit or remove a part used or a device. They are opened from the buttons of the
// lists (bootstrap, by id), work on the part or device the screen gives them and tell the screen what was decided.
@Component({
  selector: 'app-ticket-item-dialogs',
  templateUrl: './ticket-item-dialogs.component.html',
  styles: [':host { display: block; }']
})
export class TicketItemDialogsComponent {
  @Input() parToEdit: any = {};
  @Input() deviceToEdit: any = {};
  @Output() editPart = new EventEmitter<any>();
  @Output() deletePart = new EventEmitter<any>();
  @Output() closeParts = new EventEmitter<void>();
  @Output() editDevice = new EventEmitter<any>();
  @Output() deleteDevice = new EventEmitter<any>();
  @Output() closeDevices = new EventEmitter<void>();

  constructor(public service: ApiService) { }
}
