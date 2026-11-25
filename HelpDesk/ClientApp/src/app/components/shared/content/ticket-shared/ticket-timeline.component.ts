import { Component, DoCheck, EventEmitter, Input, Output, ChangeDetectionStrategy } from '@angular/core';
import { HdAlertComponent } from '../../../../ui/hd-alert.component';
import { FormBuilder, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ApiService } from '../../../../services/api.service';
import { MirroredForm, notBlank } from './mirrored-form';
import { NgFor, NgIf, DatePipe } from '@angular/common';
import { FieldHintsDirective } from '../../field-hints.directive';

// The activity of a ticket: what was done and by whom, with the box to add more. It shows what the screen gives it
// and tells the screen what the person did; the screen keeps the data and the calls to the API.
@Component({
    selector: 'app-ticket-timeline',
    templateUrl: './ticket-timeline.component.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    styles: [':host { display: block; }'],
    imports: [HdAlertComponent, NgFor, NgIf, FormsModule, ReactiveFormsModule, FieldHintsDirective, DatePipe]
})
export class TicketTimelineComponent implements DoCheck {
  @Input() ticket: any = {};
  @Input() traces: any = [];
  @Input() trace: any = {};
  // The person pressed enter or the button to add the activity written in the box.
  @Output() add = new EventEmitter<void>();
  // The person marked or unmarked an activity as favourite ('favorito') or as the solution ('etiquetado').
  @Output() toggle = new EventEmitter<{ item: any, field: string }>();

  // The box of the activity: the text is required (spaces only do not count) and it is locked on a completed and approved ticket.
  // The button keeps checking the text itself, because a locked control never counts as invalid.
  form: MirroredForm;

  constructor(public service: ApiService, formBuilder: FormBuilder) {
    this.form = new MirroredForm(formBuilder.group({
      texto: [null, notBlank]
    }), () => this.trace);
  }

  ngDoCheck() {
    this.form.sync();
    this.form.setDisabled('texto', this.ticket.estado == this.service.ticketStatuses.completed && this.ticket.aprobadoPor != null);
  }
}
