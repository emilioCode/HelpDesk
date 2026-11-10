import { Component, EventEmitter, Input, Output } from '@angular/core';
import { ApiService } from '../../../../services/api.service';

// The activity of a ticket: what was done and by whom, with the box to add more. It shows what the screen gives it
// and tells the screen what the person did; the screen keeps the data and the calls to the API.
@Component({
  selector: 'app-ticket-timeline',
  templateUrl: './ticket-timeline.component.html',
  styles: [':host { display: block; }']
})
export class TicketTimelineComponent {
  @Input() ticket: any = {};
  @Input() traces: any = [];
  @Input() trace: any = {};
  // The person pressed enter or the button to add the activity written in the box.
  @Output() add = new EventEmitter<void>();
  // The person marked or unmarked an activity as favourite ('favorito') or as the solution ('etiquetado').
  @Output() toggle = new EventEmitter<{ item: any, field: string }>();

  constructor(public service: ApiService) { }
}
