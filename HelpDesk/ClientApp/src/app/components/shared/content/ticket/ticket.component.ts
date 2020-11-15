import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../../../services/api.service';


@Component({
  selector: 'app-ticket',
  templateUrl: './ticket.component.html',
  styles: []
})
export class TicketComponent implements OnInit {

  constructor(private service: ApiService) { }

  ngOnInit() {
  }
  
  empresas:any;
  tickets:any;
  ticket:any={};
  option;
  

  fillModal(option='add',object){
    this.option = option;
    this.ticket = object;
    if(option=='add'){
      this.ticket.idEmpresa = this.service.getUser().idEmpresa;
      this.ticket.tipoSolicitud = this.service.ticketStatus[0].value;

      // debugger;
      var element  = document.getElementById('timeline')
      element.classList.remove("active")
      document.getElementById("li_timeline").classList.remove('active')

      var element  = document.getElementById('settings')
      element.classList.remove("active")
      document.getElementById("li_settings").classList.remove('active')

      
      var element  = document.getElementById('activity')
      element.classList.add("active")
      document.getElementById("li_activity").classList.remove('active')
      document.getElementById("li_activity").classList.add('active')
    }
    console.log(this.ticket);
    console.log(this.option);
    // this.getBusiness(this.service.getUser().id);
  }

}
