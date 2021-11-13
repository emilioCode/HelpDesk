import { Component, OnInit } from '@angular/core';
declare var $:any;
import 'fullcalendar';
import { Calendar } from 'fullcalendar'

@Component({
  selector: 'app-calendar',
  templateUrl: './calendar.component.html',
  styles: []
})
export class CalendarComponent implements OnInit {

  constructor() { }

  ngOnInit() { }

  showModal(){
    $('#exampleModal').modal('show');
  }
}
