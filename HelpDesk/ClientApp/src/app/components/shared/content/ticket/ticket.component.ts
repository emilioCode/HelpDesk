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
  costumers:any;
  users:any;
  devices:any = [];
  device:any={};
  addDevice=false;
  fillModal(option='add',object){
    this.addDevice=false;
    this.option = option;
    this.ticket = object;
    if(option=='add'){
      this.ticket.idEmpresa = this.service.getUser().idEmpresa;
      // this.ticket.tipoSolicitud = this.service.getStatuses()[0].value;
      this.ticket.tipoSolicitud= '';
      this.ticket.tipoServicio = '';
      this.ticket.idCliente = '';
      this.ticket.idUsuario = '';
      this.ticket.estado = this.service.getStatuses()[0].value;
      // debugger;
      // var element  = document.getElementById('timeline')
      // element.classList.remove("active")
      // document.getElementById("li_timeline").classList.remove('active')

      // var element  = document.getElementById('settings')
      // element.classList.remove("active")
      // document.getElementById("li_settings").classList.remove('active')

      
      var element  = document.getElementById('activity')
      element.classList.add("active")
      document.getElementById("li_activity").classList.remove('active')
      document.getElementById("li_activity").classList.add('active')
    }
    console.log(this.ticket);
    console.log(this.option);
    this.getCostumers(this.service.getUser().id,'*')
    this.getUsers(this.service.getUser().id,'*')
  }

  getCostumers(id,option){
    this.service.isLoading = true;
    this.service.http.get(this.service.baseUrl + 'api/Costumer/'+ id + '/' + option,{headers:this.service.headers,responseType:'json'})
      .subscribe(res=>{
        this.costumers = res;
        this.service.isLoading = false;
      },error => {
        console.error(error);
        this.service.isLoading = false;
      });
  }

  getUsers(id,option){
    this.service.isLoading = true;
    this.service.http.get(this.service.baseUrl + 'api/User/'+ id + '/' + option,{headers:this.service.headers,responseType:'json'})
      .subscribe(res=>{
        this.users = res;
        this.service.isLoading = false;
      },error => {
        console.error(error);
        this.service.isLoading = false;
      });
  }

  i=0;
  addDeviceList(item){
    item.id =  this.i++;
    this.devices.push(item)
    this.addDevice =false;

    console.log( this.devices )
  }

  removeDeviceList(item){
    // var index = this.devices.filter(x=>x.id==item.id)[0];
    var array = this.devices.indexOf(item)
    this.devices.splice(array,1)

    console.log( this.devices )
  }
}
