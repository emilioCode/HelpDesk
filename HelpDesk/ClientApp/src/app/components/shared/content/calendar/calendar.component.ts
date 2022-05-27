import { Component, OnInit } from '@angular/core';
import { Event } from '../../../../interfaces';
import { ApiService } from '../../../../services/api.service';
declare var $:any;
declare var fullCalendar: any;
@Component({
  selector: 'app-calendar',
  templateUrl: './calendar.component.html',
  styles: []
})
export class CalendarComponent implements OnInit {

  constructor(private service: ApiService) {}
  events: Event[];
  event: Event = {
    title: '',
    start: null,
    end: null,
    path: null,
    user: null,
    ticket: null
  };

  ngOnInit() {
    this.getEvents(this.service.getUser().id);
  }


  calendarInit(){
        /* initialize the calendar
     -----------------------------------------------------------------*/
    //Date for the calendar events (dummy data)
    var date = new Date()
    var d    = date.getDate(),
        m    = date.getMonth(),
        y    = date.getFullYear()
    $('#calendar').fullCalendar({
      header: {
        left  : '',//'prev,next today', 
        center: 'title',
        right : ''//'month,agendaWeek,agendaDay'
      },      
      buttonText: {
        today: 'today',
        month: 'month',
        week : 'week',
        day  : 'day'
      },
      //Random default events
      events    : this.events,
      editable  : false,
      droppable : false, // this allows things to be dropped onto the calendar !!!
      drop      : function (date, allDay) {
        // this function is called when something is dropped
      },
      eventClick: function(info){
        // console.log('Event: ');
        console.log(info);
        // this.event = info;
        document.getElementById('modalTitle').innerText = info.title;
        var html = `<p><strong>${info.user.acceso}:</strong> ${info.user.nombre}</p>`;
        let descripcion = info.ticket.descripcion? info.ticket.descripcion: '';
        html = html + `<p><strong>Descripción:</strong> ${descripcion}</p>`;
        html = html + `<p><a href="${info.path}" target="_blank" class="btn btn-primary"><i class="fa fa-print"></i></a></p>`;
        document.getElementById('modalText').innerHTML = html;
        $('#exampleModal').modal('show');
      }
    })
  
    this.focusCalendarToday(date);
    
    $('#prev').on('click', function() {
      $('#calendar').fullCalendar('prev'); // call method
      const month = ["January","February","March","April","May","June","July","August","September","October","November","December"];
      var element = document.getElementsByClassName('fc-center')[0].innerHTML.split(' ');
      const currentMonthOnCalendar = element[0].split('>')[1];
      const currentYearOnCalendar = Number(element[1].split('<')[0]);
      if(currentMonthOnCalendar == month[m] && currentYearOnCalendar == y) document.getElementsByClassName('fc-today')[1].innerHTML = `<span class="fc-day-number">(${d})</span>`;
    });

    $('#next').on('click', function() {
      $('#calendar').fullCalendar('next'); // call method
      const month = ["January","February","March","April","May","June","July","August","September","October","November","December"];
      var element = document.getElementsByClassName('fc-center')[0].innerHTML.split(' ');
      const currentMonthOnCalendar = element[0].split('>')[1];
      const currentYearOnCalendar = Number(element[1].split('<')[0]);
      if(currentMonthOnCalendar == month[m] && currentYearOnCalendar == y) document.getElementsByClassName('fc-today')[1].innerHTML = `<span class="fc-day-number">(${d})</span>`;
    });

  }


  focusCalendarToday (date: Date) {
    const d = date.getDate();
    document.getElementsByClassName('fc-today')[1].innerHTML = `<span class="fc-day-number">(${d})</span>`; 
  }


  getEvents = (idUser: number) =>{
    const request = {
      id: idUser
    }
    // Orden de servicio  = /OrderReport?value=20200105-1
    // Taller             = /OrderReport1?value=20200102-1
    // Taller (Cerrado)   = /OrderReportF?value=20200103-1
    this.service.isLoading = true;
    this.service.http.post(this.service.baseUrl + 'api/Event', request ,{headers:this.service.headers,responseType:'json'})
      .subscribe(res=>{
        this.events = res.data.map((e)=>{
          var url = "";
          var color = "";
          
          switch (e.ticket.estado) {
            case "Abierto":
              color = "#f56954"; // blue
              break;
            case "en Proceso":
              color = "#f39c12"; // yellow
              break;
            case "Completado":
              if(e.ticket.aprobadoPor){
                color = "#00a65a"; // green
              }else{
                color = "##f56954"; // red orange
              }
              break;              
            default:
              break;
          }
          if(e.ticket.tipoSolicitud == "Servicio Taller"){
            if(!e.ticket.aprobadoPor){
              url = this.service.baseUrl + 'OrderReport1?value=' + e.ticket.noSecuencia + "-" + e.ticket.idEmpresa;
            }else{
              url = this.service.baseUrl + 'OrderReportF?value=' + e.ticket.noSecuencia + "-" + e.ticket.idEmpresa;
            }
          }else{
            url = this.service.baseUrl + 'OrderReport?value=' + e.ticket.noSecuencia + "-" + e.ticket.idEmpresa;
          }

          if(e.start == null && e.end == null){
            e.start = new Date();
            e.end = new Date();
            e.title = "*(HOLD) " + e.title;

          }else if(e.start != null && e.end == null){
            e.end = new Date();
          }

          return{
            title: e.title,
            start: e.start,
            end: e.end,
            url: null,
            backgroundColor: color,
            borderColor: color,
            status: e.ticket.estado,
            user: e.user,
            ticket: e.ticket ,
            path: url
          }
        })

        this.calendarInit();
        
        this.service.isLoading = false;
      },error => {
        console.error(error);
        this.service.isLoading = false;
      });
  }
}
