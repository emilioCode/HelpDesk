import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../../../services/api.service';

@Component({
  selector: 'app-inbox',
  templateUrl: './inbox.component.html',
  styles: []
})
export class InboxComponent implements OnInit {

  constructor(private service: ApiService) { 
    this.getTickets(this.service.getUser().id,"unique")
  }

  ngOnInit() {
  }

  tickets:any;
  ticket:any={};
  option;
  costumers:any;
  users:any;
  devices:any = [];
  device:any={};
  addDevice=false;
  traces:any=[];
  trace:any={};
  costumerAPs:any =[];

  fillModal(option='add',object){
    this.addDevice=false;
    this.option = option;
    this.ticket = object;

    this.trace = {};
    if(option=='add'){
      this.ticket.idEmpresa = this.service.getUser().idEmpresa;
      // this.ticket.tipoSolicitud = this.service.getStatuses()[0].value;
      this.ticket.tipoSolicitud= '';
      this.ticket.tipoServicio = '';
      this.ticket.idCliente = '';
      this.ticket.idUsuario = '';
      this.ticket.estado = this.service.getStatuses()[0].value;
      this.ticket.fechaCreacion= new Date();
      this.devices = [];
      // debugger;
      // var element  = document.getElementById('timeline')
      // element.classList.remove("active")
      // document.getElementById("li_timeline").classList.remove('active')

      // var element  = document.getElementById('settings')
      // element.classList.remove("active")
      // document.getElementById("li_settings").classList.remove('active')

      // var element  = document.getElementById('activity')
      // element.classList.add("active")
      // document.getElementById("li_activity").classList.remove('active')
      // document.getElementById("li_activity").classList.add('active')
    }
    else{
    
        if(this.ticket.fechaInicio!=null)this.ticket.fechaInicio = new Date(this.ticket.fechaInicio).toISOString().split('T')[0];
        if(this.ticket.fechaTermino!=null)this.ticket.fechaTermino = new Date(this.ticket.fechaTermino).toISOString().split('T')[0];
      
    }

    var element  = document.getElementById('timeline')
    element.classList.remove("active")
    if(document.getElementById("li_timeline")!==null)document.getElementById("li_timeline").classList.remove('active');

    var element  = document.getElementById('activity')
    element.classList.add("active")
    document.getElementById("li_activity").classList.remove('active')
    document.getElementById("li_activity").classList.add('active')

    if(this.option == 'edit'){
      this.getDevices(this.ticket.id,this.service.getUser().idEmpresa);
      this.getTraces(this.ticket.id,this.ticket.idEmpresa);
      this.getUsersAP(this.service.getUser().id,"JUST NAME");
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
    item.idEmpresa = this.service.getUser().idEmpresa;
    item.idSolicitud = 0;
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

  getTickets(id,option){
    this.service.isLoading = true;
    this.service.http.get(this.service.baseUrl + 'api/Ticket/'+ id + '/' + option,{headers:this.service.headers,responseType:'json'})
      .subscribe(res=>{
        this.tickets = res;
        console.log(  this.tickets )
        this.service.isLoading = false;
      },error => {
        console.error(error);
        this.service.isLoading = false;
      });
  }

  getDevices(id,option){
    this.service.isLoading = true;
    this.service.http.get(this.service.baseUrl + 'api/Device/'+ id + '/' + option,{headers:this.service.headers,responseType:'json'})
      .subscribe(res=>{
        this.devices = res;
        console.log(  this.devices )
        this.service.isLoading = false;
      },error => {
        console.error(error);
        this.service.isLoading = false;
      });
  }

  addDeviceOne(item){
    if(this.service.validateTrim(item.marca)
    ||this.service.validateTrim(item.modelo) ||this.service.validateTrim(item.noSerial)){
      this.service.swal('Campos requeridos','Es necesario suministrar los datos del disposivo','info');
      return false;
    }
    item.id =  this.i++;
    item.idEmpresa = this.service.getUser().idEmpresa;
    item.idSolicitud = 0;
    this.devices.push(item)
    this.addDevice =false;

    console.log( this.devices )
  }

  getTraces(idSolicitud,idEmpresa){
    this.service.isLoading = true;
    this.service.http.get(this.service.baseUrl + 'api/Trace/'+ idSolicitud + '/' + idEmpresa,{headers:this.service.headers,responseType:'json'})
      .subscribe(res=>{
        this.traces = res.data;
        console.log(  this.traces )

        this.service.isLoading = false;
      },error => {
        console.error(error);
        this.service.isLoading = false;
      });
  }

  addTrace(){
    if(this.service.validateTrim( this.trace.texto )) return false;
    this.trace.idEmpresa = this.ticket.idEmpresa;
    this.trace.idUsuario = this.service.getUser().id;
    this.trace.idSolicitud = this.ticket.id;

    this.service.isLoading = true;
    this.service.http.post(this.service.baseUrl + 'api/Trace', this.trace ,{headers:this.service.headers,responseType:'json'})
      .subscribe(res=>{
        this.getTraces(this.ticket.id,this.ticket.idEmpresa)
        this.trace = {};
        var element = document.getElementById('divScroll');
        element.scrollTop = 500;

        this.service.isLoading = false;
      },error => {
        console.error(error);
        this.service.isLoading = false;
      });
  }

  putTrace(trace,value){
    switch (value) {
      case "favorito":
        trace.favorito = !trace.favorito;
        break;
      case "etiquetado":
        trace.etiquetado = !trace.etiquetado;
        break;
      default:
        break;
    }
    this.service.isLoading = true;
    this.service.http.put(this.service.baseUrl + 'api/Trace', trace ,{headers:this.service.headers,responseType:'json'})
      .subscribe(res=>{
        this.getTraces(this.ticket.id,this.ticket.idEmpresa)

        this.service.isLoading = false;
      },error => {
        console.error(error);
        this.service.isLoading = false;
      });
  }

  putTicket(value){
    var option =value.toUpperCase();
    var idSolicitud = this.ticket.id;
    
    if(option =='ESTADO'){
      var variable = this.ticket.estado.toUpperCase();
      switch (variable) {
        case "ABIERTO":
        var temp = this.ticket.estado;
        var response = confirm('Seguro que desea reabrir esta ticket?');
        if(!response){
    
          // this.getTickets(this.service.getUser().id,"*")
          // this.ticket = this.tickets.filter(x=>x.id == idSolicitud)[0];
          option = "NOCHANGEPLEASE";
         
        }
          break;
        case "COMPLETADO":
        debugger;
        var response = confirm('Cerrar ticket?');
        if(!response){
        
          // this.getTickets(this.service.getUser().id,"*")
          // this.ticket = this.tickets.filter(x=>x.id == idSolicitud)[0];
          // this.fillModal('edit',this.ticket)
         
          option = "NOCHANGEPLEASE";
        }
          break;
        default:
          break;
      }
    }else if(option =='USUARIO'){
      var response = confirm('Seguro que desea asignar este ticket a este usuario?');
      if(!response){
  
        // this.getTickets(this.service.getUser().id,"*")
        // this.ticket = this.tickets.filter(x=>x.id == idSolicitud)[0];
        // this.fillModal('edit',this.ticket)
        option = "NOCHANGEPLEASE";
      }
    }else if(option =='EDITAR'){

    }else if(option =='APROBAR'){
      var response = confirm('Seguro que desea confirmar como completado?');
      this.ticket.aprobadoPor = this.service.getUser().id;
      option ='EDITAR';
      if(!response){
        
        // this.getTickets(this.service.getUser().id,"*")
        // this.ticket = this.tickets.filter(x=>x.id == idSolicitud)[0];
        // this.fillModal('edit',this.ticket)
        option = "NOCHANGEPLEASE";
      }
    }else{
      this.service.swal('an option is required','','error');
      option = "NOCHANGEPLEASE";
    }
    this.service.isLoading = true;
    this.service.http.put(this.service.baseUrl + 'api/Ticket/'+option, this.ticket ,{headers:this.service.headers,responseType:'json'})
      .subscribe(res=>{
        console.log(res.data)
        this.getTickets(this.service.getUser().id,"*")
        // debugger;
        var item = res.data;
        if(item.horaInicio !=null)item.horaInicio = item.horaInicio.split('.')[0];
        if(item.horaTermino !=null)item.horaTermino = item.horaTermino.split('.')[0];
        
        this.fillModal('edit',item)
        // this.getDevices(this.ticket.id,this.service.getUser().idEmpresa);
        // this.getTraces(this.ticket.id,this.ticket.idEmpresa)

        this.service.isLoading = false;
      },error => {
        console.error(error);
        this.service.isLoading = false;
      });
  }

  edit(){
    this.putTicket('EDITAR')
  }

  aprobar(){
    this.putTicket('APROBAR')
  }

  getUsersAP(id,option){
    this.service.isLoading = true;
    this.service.http.get(this.service.baseUrl + 'api/User/'+ id + '/' + option,{headers:this.service.headers,responseType:'json'})
      .subscribe(res=>{
        this.costumerAPs = res;

        console.log( this.costumerAPs )
        this.service.isLoading = false;
      },error => {
        console.error(error);
        this.service.isLoading = false;
      });
  }
}