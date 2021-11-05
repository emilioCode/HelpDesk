import { Component, OnInit } from '@angular/core';
import { CalendarOptions } from '@fullcalendar/angular';
import { ApiService } from '../../../../services/api.service';

@Component({
  selector: 'app-calendar',
  templateUrl: './calendar.component.html',
  styles: []
})
export class CalendarComponent implements OnInit {
  
  ngOnInit() {
  }
 
  option; 
  events = [
    { id:'1',title: 'event 1', date: '2021-11-01' },
    { id:'2',title: 'event 2', date: '2021-11-02' },
    { id:'3',title: 'event 2.1', date: '2021-11-02T09:30:00', allDay : false},
    {
      id:'4',
      title  : 'event2',
      start  : '2021-11-04',
      end    : '2021-11-06'
    },
    { 
      id:'5',
      title  : 'event3',
      start  : '2021-11-10T12:30:00',
      allDay : false // will make the time show
    },

  ];

  constructor(private service: ApiService){
    console.log(this.calendarOptions.events)
  }

  calendarOptions: CalendarOptions = {
    headerToolbar: { center: 'dayGridMonth,dayGridWeek,dayGridDay' }, // the headerToolbar could be in the left and the right side too
    initialView: 'dayGridMonth', // dayGridMonth, dayGridWeek, timeGridDay, listWeek
    weekends: true, // initial value
    dateClick: this.handleDateClick.bind(this), // bind is important!
    events: this.events
  };

  handleDateClick(arg:any){
    // var array = this.handleContentByArray(this.calendarOptions.events,arg.dateStr);

    // let content = '\n';
    // array.forEach((e:any) => {
    //   content += e.title+"\n";   
    // });

    document.getElementById('buttonModalDate').click(); //clicking the hidden button
  }

  toggleWeekends() {
    this.calendarOptions.weekends = !this.calendarOptions.weekends // toggle the boolean!
  }

  handleContentByArray(array:any,filter:any){
    return array.filter((x: any)=>x.date==filter);
  }



}
