import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../../../services/api.service';
import * as signalR from '@aspnet/signalr';

@Component({
  selector: 'app-waiting',
  templateUrl: './waiting.component.html',
  styles: []
})
export class WaitingComponent implements OnInit {

  constructor(private service: ApiService) { 
    if(this.service.getLevel(this.service.getUser().acceso) < 2 ){
      alert("No tiene permisos para acceder");
      this.service.route.navigateByUrl('/');
    }else{
      this.getTickets(this.service.getUser().id,"*")
    }
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
  hubConnection: signalR.HubConnection;

  ngOnInit() {
    this.hubConnection = new signalR.HubConnectionBuilder()
    .withUrl(this.service.baseUrl+'/hub')
    .build();

    this.hubConnection.on('refresh', (component, idEmpresa,idUsuario,idOther) => {
      console.log(`component: ${component} | idEmpresa: ${idEmpresa} | idUsuario: ${idUsuario} | idOther: ${idOther}`)
      // debugger
      if( (component=='ticket' && idEmpresa == this.service.getUser().idEmpresa) || this.service.getUser().acceso =="ROOT" ){
        
        /* */
        // this.service.isLoading = true;
        this.service.http.get(this.service.baseUrl + 'api/Ticket/'+ this.service.getUser().id + '/' + '*',{headers:this.service.headers,responseType:'json'})
          .subscribe(res=>{
            this.tickets = res.filter(x=>x.idUsuario ==0);
            var id = this.ticket.id===null?0:this.ticket.id;
            if(idUsuario >0 &&  id== idOther){
              
              this.ticket = this.tickets.filter(c=>c.id==idOther)[0];
              console.log(this.ticket)
              if(this.ticket === undefined){
                this.ticket = {};
                // debugger;
                var modal = document.getElementById('modal-default');
                document.getElementById('btnClose').click();
              }else{
                this.fillModal('edit',this.ticket,false);
              }
              // debugger
              // var element = document.getElementById('divScroll');
              // element.scrollTop = element.scrollHeight + 60;
            }

            this.service.isLoading = false;
          },error => {
            console.error(error);
            this.service.isLoading = false;
          });
        /* */

      }
      
      if( component=='session' && idEmpresa == this.service.getUser().idEmpresa && idUsuario == this.service.getUser().id  ){
        if(idOther == 0 ){
          alert('Su usuario ha sido deshabilitado, comuniquese con el administrador');
          this.service.closeSession();
        }
      }
    })

    this.hubConnection.start().catch(err => console.error(err.toString()));
  }

  fillModal(option='add',object,doProcess=true){
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
      this.ticket.idUsuario = '0';
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

    if(doProcess){
      var element  = document.getElementById('timeline')
      element.classList.remove("active")
      if(document.getElementById("li_timeline")!==null)document.getElementById("li_timeline").classList.remove('active');
  
      var element  = document.getElementById('activity')
      element.classList.add("active")
      document.getElementById("li_activity").classList.remove('active')
      document.getElementById("li_activity").classList.add('active')
    }


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
    if(this.service.validateTrim(item.marca) || this.service.validateTrim(item.fallaReportada)
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
        this.tickets = res.filter(x=>x.idUsuario ==0);
        console.log(  this.tickets )
        this.service.isLoading = false;
      },error => {
        console.error(error);
        this.service.isLoading = false;
      });
  }

  add(){
    if(this.service.validate(this.ticket.tipoSolicitud)
    || this.service.validate(this.ticket.tipoServicio) || this.service.validate(this.ticket.estado)
    || this.service.validate(this.ticket.idCliente) /*|| this.service.validate(this.ticket.idUsuario)*/ ){
      this.service.swal('Campos requeridos','Asegurese de completar los datos minimos necesarios para crear la orden','warning');
      return false;
    }
    if(this.ticket.tipoSolicitud == 'Servicio a Domicilio' && this.service.validate(this.ticket.descripcion)){
      this.service.swal('Campos requerido','Favor digite falla reportada','warning');
      return false;
    }

    if(this.service.getLevel(this.service.getUser().acceso) <= 1){
      this.service.swal('Access denied','','error');
      return false;
    }

    if(this.devices.lentgh == 0 && this.ticket.tipoSolicitud =='Servicio Taller'){
      this.service.swal('Must have a device as well','','warning');
      return false;
    }
    this.ticket.idUsuario = parseInt(this.ticket.idUsuario);
    console.log('--TICKET REQUEST--')
    console.log(this.ticket)
    this.service.isLoading = true;
     this.service.http.post(this.service.baseUrl + 'api/Ticket',this.ticket,{headers:this.service.headers,responseType:'json'})
      .subscribe(res=>{
      console.log( res )
      this.service.swal(res.title,res.message,res.icon);
      if(res.code=="1") {
        this.option = 'edit';
        var idSolicitud = res.data.id;
        if(this.devices.length > 0)this.addDevices(idSolicitud);
        this.ticket.noSecuencia = res.data.noSecuencia;
        this.ticket.fechaCreacion = res.data.fechaCreacion;
        this.ticket.fechaInicio = res.data.fechaInicio;
        this.ticket.fechaTermino = res.data.fechaTermino;
        this.ticket.horaTermino = res.data.horaTermino;
        this.service.swal('No:'+ this.ticket.noSecuencia,'','success')
      }
      this.service.isLoading =false;
      },error => {
        console.error(error);
        this.service.isLoading =false;
      });
  }

  addDevices(idSolicitud){
    if(this.devices.lentgh == 0){
      this.service.swal('Must have a device as well','','warning');
      return false;
    }
    this.service.isLoading = true;
    var temp = this.devices;
    this.devices.forEach(element => {
      element.idSolicitud = idSolicitud;
    });
    console.log('devices')
    console.log(this.devices)
     this.service.http.post(this.service.baseUrl + 'api/Device/PostArray',this.devices,{headers:this.service.headers,responseType:'json'})
      .subscribe(res=>{
      console.log( res )
      
      if(res.code=="1") {
        console.log('devices saved')
        

        this.getDevices(idSolicitud,this.service.getUser().idEmpresa);
        // var object = this.devices.filter(x=>x.idSolicitud==idSolicitud)[0];
        // console.log(""+this.option+" "+object)
        // this.fillModal(this.option,object);
      }else{
        this.devices = temp;
        this.service.swal(res.title,res.message,res.icon);
      }
      this.service.isLoading =false;
      },error => {
        console.error(error);
        this.service.isLoading =false;
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

  clearDevicesIf(){
    if(this.ticket.tipoSolicitud =='Servicio a Domicilio')this.devices = [];
    this.ticket.descripcion = '';
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
        // this.getTraces(this.ticket.id,this.ticket.idEmpresa)
        this.hubConnection.invoke('refresh', 'ticket',this.ticket.idEmpresa,this.ticket.idUsuario,this.ticket.id===undefined?0:this.ticket.id)

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
      option ='EDITAR';
      if(!response){
        
        // this.getTickets(this.service.getUser().id,"*")
        // this.ticket = this.tickets.filter(x=>x.id == idSolicitud)[0];
        // this.fillModal('edit',this.ticket)
        option = "NOCHANGEPLEASE";
      }else{
        this.ticket.aprobadoPor = this.service.getUser().id;
      }
    }else{
      this.service.swal('an option is required','','error');
      option = "NOCHANGEPLEASE";
    }
    this.service.isLoading = true;
    this.service.http.put(this.service.baseUrl + 'api/Ticket/'+option, this.ticket ,{headers:this.service.headers,responseType:'json'})
      .subscribe(res=>{
        console.log(res.data)
        // this.getTickets(this.service.getUser().id,"*")
        
        this.hubConnection.invoke('refresh', 'ticket',this.ticket.idEmpresa,this.ticket.idUsuario,this.ticket.id===undefined?0:this.ticket.id)
        // this.ticket.id = 0; // para que no explote al llegar en la bandeja de waitting
        // debugger;
        var item = res.data;
        if(item.horaInicio !=null)item.horaInicio = item.horaInicio.split('.')[0];
        if(item.horaTermino !=null)item.horaTermino = item.horaTermino.split('.')[0];
        
        this.fillModal('edit',item,true)
        // this.getDevices(this.ticket.id,this.service.getUser().idEmpresa);
        // this.getTraces(this.ticket.id,this.ticket.idEmpresa)

        this.service.isLoading = false;
      },error => {
        console.error(error);
        this.service.isLoading = false;
      });
  }
      // to Check!
  //   parseDate(dateString: string): any {
  //     if (dateString) {// new Date().toISOString().split('T')[0];
  //         return new Date( dateString ).toISOString().split('T')[0];
  //     }
  //     return null;
  // }

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
