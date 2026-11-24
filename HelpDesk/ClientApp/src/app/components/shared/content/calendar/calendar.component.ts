import { Component, OnInit, OnDestroy, ChangeDetectionStrategy } from '@angular/core';
import { Calendar } from '@fullcalendar/core';
import dayGridPlugin from '@fullcalendar/daygrid';
import listPlugin from '@fullcalendar/list';
import esLocale from '@fullcalendar/core/locales/es';
import { Event } from '../../../../interfaces';
import { ApiService } from '../../../../services/api.service';
declare var $:any;
@Component({
    selector: 'app-calendar',
    templateUrl: './calendar.component.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    styles: []
})
export class CalendarComponent implements OnInit, OnDestroy {

  constructor(public service: ApiService) {}
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


  calendar: Calendar | null = null;

  ngOnDestroy() {
    if (this.calendar) { this.calendar.destroy(); this.calendar = null; }
  }

  // Month grid on tablets and desktops, a plain list of the month on phones
  private isPhone() { return window.innerWidth < 768; }
  private phone = false;

  calendarInit(){
    const el = document.getElementById('calendar');
    if (!el) return;
    if (this.calendar) this.calendar.destroy();
    this.phone = this.isPhone();
    // An order lasts from its start until it ends (or today). The grid draws that span; the phone list shows each order once, on its start day.
    const events = this.phone ? this.events.map(e => ({ ...e, end: null })) : this.events;
    this.calendar = new Calendar(el, {
      plugins: [dayGridPlugin, listPlugin],
      locale: esLocale,
      initialView: this.phone ? 'listMonth' : 'dayGridMonth',
      windowResize: () => { if (this.isPhone() !== this.phone) this.calendarInit(); },
      displayEventTime: false,
      headerToolbar: { left: 'prev,next today', center: 'title', right: '' },
      buttonText: { today: 'Hoy' },
      height: 'auto',
      dayMaxEvents: 3,
      moreLinkText: n => '+' + n + ' más',
      noEventsText: 'No hay órdenes este mes',
      events: events as any,
      editable: false,
      eventClick: (info) => {
        info.jsEvent.preventDefault();
        const p: any = info.event.extendedProps;
        document.getElementById('modalTitle').textContent = info.event.title;
        const box = document.getElementById('modalText');
        box.textContent = '';
        const line = (label: string, text: string) => {
          const para = document.createElement('p');
          const strong = document.createElement('strong');
          strong.textContent = label + ': ';
          para.append(strong, text);
          box.appendChild(para);
        };
        line(p.user.acceso, p.user.nombre);
        line('Descripción', p.ticket.descripcion ? p.ticket.descripcion : '');
        const para = document.createElement('p');
        const link = document.createElement('a');
        link.href = p.path; link.target = '_blank'; link.className = 'btn btn-primary';
        link.innerHTML = '<i class="fa fa-print"></i> Imprimir';
        para.appendChild(link); box.appendChild(para);
        $('#exampleModal').modal('show');
      }
    });
    this.calendar.render();
  }


  getEvents = (idUser: number) =>{
    const request = {
      id: idUser
    }
    // Orden de servicio  = /OrderReport?value=20200105-1
    // Taller             = /OrderReport1?value=20200102-1
    // Taller (Cerrado)   = /OrderReportF?value=20200103-1
    this.service.isLoading = true;
    this.service.post('Event', request)
      .subscribe(res=>{
        // One reading of the clock for all the events: with one new Date() per event the durations differed by a millisecond
        // and the calendar, which sorts equal starts by duration, showed the same events in a different order on each load.
        const now = new Date().getTime();
        this.events = res.data.map((e)=>{
          var url = "";
          var cls = "hd-ev-open";
          
          switch (e.ticket.estado) {
            case this.service.ticketStatuses.open:
              cls = "hd-ev-open";
              break;
            case this.service.ticketStatuses.inProgress:
              cls = "hd-ev-progress";
              break;
            case this.service.ticketStatuses.completed:
              if(e.ticket.aprobadoPor){
                cls = "hd-ev-approved";
              }else{
                cls = "hd-ev-done";
              }
              break;              
            default:
              break;
          }
          if(e.ticket.tipoSolicitud == this.service.requestTypes.workshop){
            if(!e.ticket.aprobadoPor){
              url = this.service.baseUrl + 'OrderReport1?value=' + e.ticket.noSecuencia + "-" + e.ticket.idEmpresa;
            }else{
              url = this.service.baseUrl + 'OrderReportF?value=' + e.ticket.noSecuencia + "-" + e.ticket.idEmpresa;
            }
          }else{
            url = this.service.baseUrl + 'OrderReport?value=' + e.ticket.noSecuencia + "-" + e.ticket.idEmpresa;
          }

          if(e.start == null && e.end == null){
            e.start = new Date(now);
            e.end = new Date(now);
            e.title = "*(HOLD) " + e.title;

          }else if(e.start != null && e.end == null){
            e.end = new Date(now);
          }

          return{
            title: e.title,
            start: e.start,
            end: e.end,
            url: null,
            classNames: [cls],
            status: e.ticket.estado,
            user: e.user,
            ticket: e.ticket ,
            path: url
          }
        })

        this.calendarInit();
        
        this.service.isLoading = false;
      },error => {
        this.service.handleHttpError(error);
        this.service.isLoading = false;
      });
  }
}
