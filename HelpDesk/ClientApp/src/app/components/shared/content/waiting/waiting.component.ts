import { Component, DoCheck, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { ApiService } from '../../../../services/api.service';
// import * as signalR from '@aspnet/signalr';
import * as signalR from '@microsoft/signalr';

@Component({
  selector: 'app-waiting',
  templateUrl: './waiting.component.html',
  styles: []
})
export class WaitingComponent implements OnInit, OnDestroy, DoCheck {

  // One screen for the lists of tickets. The route says which one it is:
  //  - mode: 'pending' (not assigned yet), 'inbox' (the tickets of the person, not approved yet), 'finished' (completed, not approved yet)
  //    or 'all' (every ticket; the only one that creates orders and customers)
  //  - requestType: 'workshop', 'onSite', or nothing for every type (only for 'pending')
  //  - title: the heading of the list
  mode: string = 'pending';
  requestType: string = null;
  title: string = '';

  constructor(private service: ApiService, private activatedRoute: ActivatedRoute, private formBuilder: FormBuilder) { 
    this.initializeForms();
    const data = this.activatedRoute.snapshot.data;
    this.requestType = data.requestType ? this.service.requestTypes[data.requestType] : null;
    this.title = data.title;
    this.mode = data.mode || 'pending';
    // The inbox is open to every level (it shows only the tickets of the person); the other lists need the moderator level.
    if(this.mode != 'inbox' && this.service.getLevel(this.service.getUser().acceso) < 2 ){
      alert(this.service.messages.noPermissionToAccess);
      this.service.route.navigateByUrl('/');
    }else{
      this.getTickets(this.service.getUser().id, this.mode == 'inbox' ? 'unique' : '*')
    }
  }

  // The forms. The data of the screen stays in the plain objects (ticket, device, costumer), which the code of the screen reads and
  // changes everywhere; each form keeps its controls and the validation, and both are kept equal (see ngDoCheck and watch).
  ticketForm: FormGroup;
  deviceForm: FormGroup;
  costumerForm: FormGroup;

  // Limits of the new device and of the new customer are the ones of the server (DeviceValidator, CustomerValidator) and of the columns.
  private initializeForms(){
    this.ticketForm = this.formBuilder.group({
      idCliente: [null, Validators.required],
      tipoSolicitud: [null, Validators.required],
      tipoServicio: [null, Validators.required],
      fechaInicio: [null],
      horaInicio: [null],
      fechaTermino: [null],
      horaTermino: [null],
      descripcion: [null],
      estado: [null],
      idUsuario: [null],
      aprobadoPor: [null]
    });
    this.deviceForm = this.formBuilder.group({
      descripcion: [null],
      fallaReportada: [null, Validators.maxLength(80)],
      marca: [null, Validators.maxLength(50)],
      modelo: [null, Validators.maxLength(50)],
      noSerial: [null, Validators.maxLength(50)]
    });
    this.costumerForm = this.formBuilder.group({
      nombre: [null, [Validators.required, Validators.maxLength(100)]],
      rnc: [null, [Validators.required, Validators.maxLength(30)]],
      contacto: [null, Validators.maxLength(80)],
      telefono: [null, Validators.maxLength(20)],
      extension: [null, Validators.maxLength(10)],
      correo: [null, Validators.maxLength(50)],
      departamento: [null, Validators.maxLength(60)],
      idEmpresa: [null],
      direccion: [null]
    });
    this.watch(this.ticketForm, () => this.ticket);
    this.watch(this.deviceForm, () => this.device);
    this.watch(this.costumerForm, () => this.costumer);
  }

  // What the person types in a control goes to the object of the screen right away (before the code of the change event runs).
  private watch(form: FormGroup, model: () => any){
    Object.keys(form.controls).forEach(name => {
      form.get(name).valueChanges.subscribe(value => { model()[name] = value; });
    });
  }

  // What the code of the screen leaves in the objects goes to the controls, with the same rules the template used to apply with [disabled].
  ngDoCheck(){
    this.mirror(this.ticketForm, this.ticket);
    this.mirror(this.deviceForm, this.device);
    this.mirror(this.costumerForm, this.costumer);
    this.applyDisabled();
  }

  private mirror(form: FormGroup, model: any){
    Object.keys(form.controls).forEach(name => {
      const control = form.get(name);
      const value = model && model[name] !== undefined ? model[name] : null;
      if(control.value !== value){
        control.setValue(value, { emitEvent: false });
      }
    });
  }

  private setDisabled(form: FormGroup, name: string, disabled: boolean){
    const control = form.get(name);
    if(disabled && control.enabled){
      control.disable({ emitEvent: false });
    }else if(!disabled && control.disabled){
      control.enable({ emitEvent: false });
    }
  }

  private applyDisabled(){
    const t = this.ticket || {};
    const level = this.service.getLevel(this.service.getUser().acceso);
    const adding = this.option == 'add';
    const closedAndApproved = t.estado == this.service.ticketStatuses.completed && t.aprobadoPor != null;
    const dates = adding || closedAndApproved;
    ['fechaInicio','horaInicio','fechaTermino','horaTermino'].forEach(n => this.setDisabled(this.ticketForm, n, dates));
    // What an administrator can still adjust: not on a completed ticket, and not by lower levels once the ticket exists.
    const locked = (!adding && 2 > level) || t.estado == this.service.ticketStatuses.completed;
    const finished = this.mode == 'finished';
    this.setDisabled(this.ticketForm, 'idCliente', locked);
    this.setDisabled(this.ticketForm, 'tipoSolicitud', !adding);
    this.setDisabled(this.ticketForm, 'tipoServicio', finished ? !adding : locked);
    this.setDisabled(this.ticketForm, 'descripcion', finished ? !adding : locked);
    this.setDisabled(this.ticketForm, 'idUsuario', this.mode != 'inbox' && (level < 2 || (!adding && this.service.getUser().id == t.idUsuario && level < 1)));
    this.setDisabled(this.ticketForm, 'aprobadoPor', true);
    // Only a person who can create customers sees the new customer form enabled.
    const canCreateCostumers = level > 1;
    Object.keys(this.costumerForm.controls).forEach(n => this.setDisabled(this.costumerForm, n, !canCreateCostumers));
  }

  tickets:any;
  ticket:any={};
  option;
  costumers:any;
  costumer:any={};
  users:any;
  devices:any = [];
  device:any={};
  addDevice=false;
  traces:any=[];
  trace:any={};
  costumerAPs:any =[];
  parts:any = [];
  part:any={};
  addPart=false;
  hubConnection: signalR.HubConnection;

  // Each screen opens its own real-time connection, so it closes it when the screen goes away.
  ngOnDestroy() {
    this.hubConnection.stop().catch(() => {});
  }

  ngOnInit() {
    this.hubConnection = new signalR.HubConnectionBuilder()
    .withUrl(this.service.baseUrl+'hub', { accessTokenFactory: () => this.service.getToken() })
    //.withUrl('/hub'//this.service.baseUrl+'/hub'
    // ,{
    //   skipNegotiation: true,
    //   transport: signalR.HttpTransportType.WebSockets, // | signalR.HttpTransportType.LongPolling
    // })
    // .configureLogging(signalR.LogLevel.Debug)
    .build();

    this.hubConnection.on('refresh', (component, idEmpresa,idUsuario,idOther) => {
      // console.log(`component: ${component} | idEmpresa: ${idEmpresa} | idUsuario: ${idUsuario} | idOther: ${idOther}`)
      // debugger
      if( (component=='ticket' && idEmpresa == this.service.getUser().idEmpresa) || this.service.getUser().acceso ==this.service.roles.root ){
        
        /* */
        // this.service.isLoading = true;
        this.service.get('Ticket/'+ this.service.getUser().id + '/' + (this.mode == 'inbox' ? 'unique' : '*'))
          .subscribe(res=>{
            this.tickets = this.listed(res);
            var id = this.ticket.id===null?0:this.ticket.id;
            if((this.mode != 'inbox' || idUsuario >0) &&  id== idOther){
              
              this.ticket = this.tickets.filter(c=>c.id==idOther)[0];
              // console.log(this.ticket)
              if(this.ticket === undefined && this.mode != 'all'){
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
            this.service.handleHttpError(error);
            this.service.isLoading = false;
          });
        /* */

      }
      
      // Only the screen that creates orders keeps its list of customers up to date.
      if( this.mode == 'all' && component=='costumer' && idEmpresa == this.service.getUser().idEmpresa ){
        this.service.get('Costumer/'+ this.service.getUser().id + '/' + '*')
          .subscribe(res=>{
            this.costumers = res;
            this.service.isLoading = false;
          },error => {
            this.service.handleHttpError(error);
            this.service.isLoading = false;
          });
      }

      if( component=='session' && idEmpresa == this.service.getUser().idEmpresa && idUsuario == this.service.getUser().id  ){
        if(idOther == 0 ){
          alert(this.service.messages.userDisabled);
          this.service.closeSession();
        }
      }
    })

    this.hubConnection.start().catch(err => console.error(err.toString()));
  }

  savedDates: any;

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
      this.parts = [];
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
        this.savedDates = { fechaInicio: this.ticket.fechaInicio, horaInicio: this.ticket.horaInicio, fechaTermino: this.ticket.fechaTermino, horaTermino: this.ticket.horaTermino };
      
    }

    if(doProcess){
      // var element  = document.getElementById('timeline')
      // element.classList.remove("active")
      if(document.getElementById("li_timeline")!==null)document.getElementById("li_timeline").classList.remove('active');
      if(document.getElementById("li_settings")!==null)document.getElementById("li_settings").classList.remove('active');
  
      var element  = document.getElementById('activity')
      element.classList.add("active")
      document.getElementById("li_activity").classList.remove('active')
      document.getElementById("li_activity").classList.add('active')
    }


    if(this.option == 'edit'){
      this.getDevices(this.ticket.id);
      this.getParts(this.ticket.id,this.service.getUser().idEmpresa);
      this.getTraces(this.ticket.id,this.ticket.idEmpresa);
      this.getUsersAP(this.service.getUser().id,"JUST NAME");
    } 
    // console.log(this.ticket);
    // console.log(this.option);
    this.getCostumers(this.service.getUser().id,'*')
    this.getUsers(this.service.getUser().id,'*')
  }

  getCostumers(id,option){
    this.service.isLoading = true;
    this.service.get('Costumer/'+ id + '/' + option)
      .subscribe(res=>{
        this.costumers = res;
        this.service.isLoading = false;
      },error => {
        this.service.handleHttpError(error);
        this.service.isLoading = false;
      });
  }

  getUsers(id,option){
    this.service.isLoading = true;
    this.service.get('User/'+ id + '/' + option)
      .subscribe(res=>{
        this.users = res;
        this.service.isLoading = false;
      },error => {
        this.service.handleHttpError(error);
        this.service.isLoading = false;
      });
  }

  i=0;
  // Only reachable while an order is being created (option 'add'), which only the 'all' screen does.
  addDeviceList(item){
    if(this.ticket.tipoSolicitud ==this.service.requestTypes.onSite){
      this.addDeviceOne(item);
    }else if(this.ticket.tipoSolicitud ==this.service.requestTypes.workshop){
      if(this.service.validateTrim(item.marca) || this.service.validateTrim(item.fallaReportada)
      ||this.service.validateTrim(item.modelo) ||this.service.validateTrim(item.noSerial)){
        this.service.swal(this.service.messages.titleRequiredFields,this.service.messages.deviceDataRequired,'info');
        return false;
      }
      if(this.deviceForm.invalid){
        this.service.swal(this.service.messages.titleRequiredFields,this.service.messages.deviceDataTooLong,'info');
        return false;
      }
      item.id =  this.i++;
      item.idEmpresa = this.service.getUser().idEmpresa;
      item.idSolicitud = 0;
      this.devices.push(item)
      this.addDevice =false;
    }
  }

  removeDeviceList(item){
    // var index = this.devices.filter(x=>x.id==item.id)[0];
    var array = this.devices.indexOf(item)
    this.devices.splice(array,1)

    // console.log( this.devices )
  }

  // The tickets this screen lists.
  private listed(tickets){
    switch(this.mode){
      case 'inbox':
        return tickets.filter(t=>t.aprobadoPor == null);
      case 'finished':
        return tickets.filter(x=>x.estado == this.service.ticketStatuses.completed && x.aprobadoPor==null);
      case 'all':
        return tickets;
      default:
        // No technician yet, and of the type this screen shows.
        return tickets.filter(x=>x.idUsuario ==0 && (this.requestType == null || x.tipoSolicitud == this.requestType));
    }
  }

  getTickets(id,option){
    this.service.isLoading = true;
    this.service.get('Ticket/'+ id + '/' + option)
      .subscribe(res=>{
        this.tickets = this.listed(res);
        // console.log(  this.tickets )
        this.tickets.forEach(element => {
          if(element.horaInicio != null)element.horaInicio = element.horaInicio; //this.service.fillZeroWithNumber(element.horaInicio/*.value.hours*/) + ":" + this.service.fillZeroWithNumber(element.horaInicio.value.minutes) + ":" + this.service.fillZeroWithNumber(element.horaInicio.value.seconds); 
          if(element.horaTermino != null)element.horaTermino = element.horaTermino; //this.service.fillZeroWithNumber(element.horaTermino/*.value.hours*/) + ":" + this.service.fillZeroWithNumber(element.horaTermino.value.minutes) + ":" + this.service.fillZeroWithNumber(element.horaTermino.value.seconds); 
        });
        this.service.isLoading = false;
      },error => {
        this.service.handleHttpError(error);
        this.service.isLoading = false;
      });
  }

  add(){
    // The customer, the type of request and the type of service are required (the form says so); the state is always set.
    if(this.ticketForm.invalid || this.service.validate(this.ticket.estado)){
      this.service.swal(this.service.messages.titleRequiredFields,this.service.messages.completeMinimumOrderData,'warning');
      return false;
    }
    if(this.ticket.tipoSolicitud == this.service.requestTypes.onSite && this.service.validate(this.ticket.descripcion)){
      this.service.swal(this.service.messages.titleRequiredFields,this.service.messages.enterReportedFailure,'warning');
      return false;
    }

    if(this.service.getLevel(this.service.getUser().acceso) <= 1){
      this.service.swal(this.service.messages.titleAccessDenied,'','error');
      return false;
    }

    if(this.devices.lentgh == 0 && this.ticket.tipoSolicitud ==this.service.requestTypes.workshop){
      this.service.swal(this.service.messages.mustHaveDevice,'','warning');
      return false;
    }
    this.ticket.idUsuario = parseInt(this.ticket.idUsuario);
    // console.log('--TICKET REQUEST--')
    // console.log(this.ticket)
    this.service.isLoading = true;
     this.service.post('Ticket', this.ticket)
      .subscribe(res=>{
      // console.log( res )
      this.service.swal(res.title,res.message,res.icon);
      if(res.code=="1") {
        this.option = 'edit';
        var idSolicitud = res.data.id;
        if(this.devices.length > 0)this.addDevices(idSolicitud);
        this.hubConnection.invoke('refresh', 'ticket',this.ticket.idEmpresa,this.ticket.idUsuario,0)
        this.ticket.id = res.data.id;
        this.ticket.noSecuencia = res.data.noSecuencia;
        this.ticket.fechaCreacion = res.data.fechaCreacion;
        this.ticket.fechaInicio = res.data.fechaInicio;
        this.ticket.fechaTermino = res.data.fechaTermino;
        this.ticket.horaTermino = res.data.horaTermino;
        this.service.swal('No:'+ this.ticket.noSecuencia,'','success')
      }
      this.service.isLoading =false;
      },error => {
        this.service.handleHttpError(error);
        this.service.isLoading =false;
      });
  }

  addDevices(idSolicitud){
    if(this.devices.lentgh == 0){
      this.service.swal(this.service.messages.mustHaveDevice,'','warning');
      return false;
    }
    this.service.isLoading = true;
    var temp = this.devices;
    this.devices.forEach(element => {
      element.idSolicitud = idSolicitud;
    });
    // console.log('devices')
    // console.log(this.devices)
     this.service.post('Device/PostArray', this.devices)
      .subscribe(res=>{
      // console.log( res )
      
      if(res.code=="1") {
        // console.log('devices saved')
        

        // The devices of a new order are already in the list; nothing to ask for.
      }else{
        this.devices = temp;
        this.service.swal(res.title,res.message,res.icon);
      }
      this.service.isLoading =false;
      },error => {
        this.service.handleHttpError(error);
        this.service.isLoading =false;
      });
  }

  getDevices(id){
    this.service.isLoading = true;
    // The inbox has always asked with the company in the address too.
    this.service.get('Device/'+ id + (this.mode == 'inbox' ? '/' + this.service.getUser().idEmpresa : ''))
      .subscribe(res=>{
        this.devices = res;
        // console.log(  this.devices )
        this.service.isLoading = false;
      },error => {
        this.service.handleHttpError(error);
        this.service.isLoading = false;
      });
  }

  clearDevicesIf(){
    if(this.ticket.tipoSolicitud ==this.service.requestTypes.onSite)this.devices = [];
    this.ticket.descripcion = '';
  }

  getTraces(idSolicitud,idEmpresa){
    this.service.isLoading = true;
    this.service.get('Trace/'+ idSolicitud + '/' + idEmpresa)
      .subscribe(res=>{
        this.traces = res.data;
        // console.log(  this.traces )

        this.service.isLoading = false;
      },error => {
        this.service.handleHttpError(error);
        this.service.isLoading = false;
      });
  }

  addTrace(){
    if(this.service.validateTrim( this.trace.texto )) return false;
    this.trace.idEmpresa = this.ticket.idEmpresa;
    this.trace.idUsuario = this.service.getUser().id;
    this.trace.idSolicitud = this.ticket.id;

    this.service.isLoading = true;
    this.service.post('Trace', this.trace)
      .subscribe(res=>{
        if(this.mode == 'inbox' || this.mode == 'all'){
          // The inbox and the full list let the notice bring the activities back instead of asking for them.
          this.hubConnection.invoke('refresh', 'ticket',this.ticket.idEmpresa,this.ticket.idUsuario,this.ticket.id===undefined?0:this.ticket.id)
        }else{
          this.getTraces(this.ticket.id,this.ticket.idEmpresa)
        }
        this.trace = {};
        var element = document.getElementById('divScroll');
        element.scrollTop = (this.mode == 'inbox' || this.mode == 'all') ? element.scrollHeight + 60 : 500;

        this.service.isLoading = false;
      },error => {
        this.service.handleHttpError(error);
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
    this.service.post('Trace/Put', trace)
      .subscribe(res=>{
        // this.getTraces(this.ticket.id,this.ticket.idEmpresa)
        this.hubConnection.invoke('refresh', 'ticket',this.ticket.idEmpresa,this.ticket.idUsuario,this.ticket.id===undefined?0:this.ticket.id)

        this.service.isLoading = false;
      },error => {
        this.service.handleHttpError(error);
        this.service.isLoading = false;
      });
  }

  putTicket(value){
    var option =value.toUpperCase();
    var idSolicitud = this.ticket.id;
    
    if(option =='ESTADO'){
      var variable = this.ticket.estado.toUpperCase();
      if(this.ticket.idUsuario == '0'){
        alert(this.service.messages.ticketWithoutTechnician);
        variable = '';
      }
      switch (variable) {

        case this.service.ticketStatuses.open.toUpperCase():
        var temp = this.ticket.estado;
        var response = confirm(this.service.messages.confirmReopenTicket);
        if(!response){
    
          // this.getTickets(this.service.getUser().id,"*")
          // this.ticket = this.tickets.filter(x=>x.id == idSolicitud)[0];
          option = "NOCHANGEPLEASE";
         
        }
        case this.service.ticketStatuses.inProgress.toUpperCase():
          // var response = confirm(this.service.messages.confirmReopenTicket);
          // if(!response){
      
          //   // this.getTickets(this.service.getUser().id,"*")
          //   // this.ticket = this.tickets.filter(x=>x.id == idSolicitud)[0];
          //   option = "NOCHANGEPLEASE";
          
          // }
          break;
        case this.service.ticketStatuses.completed.toUpperCase():
        
        // var response = confirm('Utilizó partes o repuestos?\nDesea cerrar la orden?');
        // if(!response){
         
        //   option = "NOCHANGEPLEASE";
        // }
          break;
        default:
          option = "NOCHANGEPLEASE";
          break;
      }
    }else if(option =='USUARIO'){
      // A new order (no number yet) is not assigned until it is created.
      if(!this.ticket.noSecuencia){
        return false;
      }

      var response = confirm(this.service.messages.confirmAssignTicket);
      if(!response){
  
        // this.getTickets(this.service.getUser().id,"*")
        // this.ticket = this.tickets.filter(x=>x.id == idSolicitud)[0];
        // this.fillModal('edit',this.ticket)
        option = "NOCHANGEPLEASE";
      }
    }else if(option =='EDITAR'){

    }else if(option =='APROBAR'){
      var response = confirm(this.service.messages.confirmCompleteTicket);
      // option ='EDITAR';
      if(!response){
        
        // this.getTickets(this.service.getUser().id,"*")
        // this.ticket = this.tickets.filter(x=>x.id == idSolicitud)[0];
        // this.fillModal('edit',this.ticket)
        option = "NOCHANGEPLEASE";
      }else{
        this.ticket.aprobadoPor = this.service.getUser().id;
      }
    }else{
      this.service.swal(this.service.messages.optionRequired,'','error');
      option = "NOCHANGEPLEASE";
    }
    this.service.isLoading = true;
    this.service.post('Ticket/EDIT/'+option, this.ticket)
      .subscribe(res=>{
        // console.log(res.data)

        if(res.code == '5'){
          this.ticket.estado  = temp;
          this.service.swal(res.title,res.message,res.icon);
        }
         
        // this.getTickets(this.service.getUser().id,"*")
        this.hubConnection.invoke('refresh', 'ticket',this.ticket.idEmpresa,this.ticket.idUsuario,this.ticket.id===undefined?0:this.ticket.id)
        // debugger;
        var item = res.data;
        
        if(item.horaInicio !=null)item.horaInicio = item.horaInicio.split('.')[0];
        if(item.horaTermino !=null)item.horaTermino = item.horaTermino.split('.')[0];          
                
        this.fillModal('edit',item,false)
        // this.getDevices(this.ticket.id,this.service.getUser().idEmpresa);
        // this.getTraces(this.ticket.id,this.ticket.idEmpresa)
        

        this.service.isLoading = false;
      },error => {
        this.service.handleHttpError(error);
        this.ticket.estado = temp;
        if (this.savedDates) {
          this.ticket.fechaInicio = this.savedDates.fechaInicio;
          this.ticket.horaInicio = this.savedDates.horaInicio;
          this.ticket.fechaTermino = this.savedDates.fechaTermino;
          this.ticket.horaTermino = this.savedDates.horaTermino;
        }
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

  editAsAdministrator(){
    var todo='EDITAR';
    if(this.option =='edit' && this.ticket.estado.toUpperCase() == this.service.ticketStatuses.completed.toUpperCase()){
      this.service.swal(this.service.messages.titleClosedOrder,this.service.messages.closedOrderAdjustment, 'warning');
      this.ticket.estado = "NOCHANGEPLEASE";
      todo = 'ESTADO';
    }else if(this.option =='edit'){
      if( !(this.service.getLevel(this.service.getUser().acceso)>1) ){
        this.service.swal(this.service.messages.titleAdjustmentNotApplied,this.service.messages.noAdminPermissionToAdjust, 'error');
        this.ticket.estado = "NOCHANGEPLEASE";
        todo = 'ESTADO';
      }
  
      this.putTicket(todo);
    }
  }

  aprobar(){
    this.putTicket('APROBAR')
  }

  setEstatus(){
    if(this.ticket.estado.toUpperCase() == this.service.ticketStatuses.completed.toUpperCase() && this.traces.length == 0){
      this.service.swal(this.service.messages.titleRequiredActivities,this.service.messages.activityRequired,'warning');
      this.ticket.estado = "NOCHANGEPLEASE";
      this.putTicket('ESTADO');
      document.getElementById("li_activity").classList.remove('active');
      document.getElementById("li_timeline").classList.add('active');
      
      document.getElementById("activity").classList.remove('active');
      document.getElementById("timeline").classList.add('active');
    } else if(this.ticket.estado.toUpperCase() == this.service.ticketStatuses.completed.toUpperCase()){
      this.service.swal({
        title: this.service.messages.usedPartsQuestion,
        text: this.service.messages.addPartQuestion,
        type: 'question',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: this.service.messages.yes,
        cancelButtonText:this.service.messages.noOrderIsComplete
      }).then((result) => {
        // debugger;
        var res = String(result.dismiss);
        if(res =="undefined") res = String(result.value);
        // console.log(result);
        switch(res){
  
          case 'overlay':
          this.ticket.estado = "NOCHANGEPLEASE";
          this.putTicket('ESTADO');
            break;
  
          case 'cancel':
          this.putTicket('ESTADO');
            break;
  
          case 'true':
          this.ticket.estado = "NOCHANGEPLEASE";
          /* */
          document.getElementById("li_activity").classList.remove('active')
          document.getElementById("li_settings").classList.add('active')
          
          document.getElementById("activity").classList.remove('active')
          document.getElementById("settings").classList.add('active')
          
          /* */
          this.putTicket('ESTADO');
            break;
  
          default:
          this.ticket.estado = "NOCHANGEPLEASE";
          this.putTicket('ESTADO');
            break;
        }
  
  
      })
    }else{
      this.putTicket('ESTADO');
    }
  }

  getUsersAP(id,option){
    this.service.isLoading = true;
    this.service.get('User/'+ id + '/' + option)
      .subscribe(res=>{
        this.costumerAPs = res;

        // console.log( this.costumerAPs )
        this.service.isLoading = false;
      },error => {
        this.service.handleHttpError(error);
        this.service.isLoading = false;
      });
  }

  chooseClient(client){
    this.ticket.idCliente = String(client.id);
  }

  // A new customer starts from an empty form.
  newClient(){
    this.costumer = {};
  }

  addClient(){
    this.costumer.idEmpresa = this.service.getUser().idEmpresa;

    if(this.service.getLevel(this.service.getUser().acceso) <= 1){
      this.service.swal(this.service.messages.titleAccessDenied,'','error');
      return false;
    }
    this.service.isLoading = true;
     // The empty fields are not sent, as before.
     const body = Object.assign({}, this.costumer);
     Object.keys(body).forEach(k => { if(body[k] === '' || body[k] === null) delete body[k]; });
     this.service.post('Costumer', body)
      .subscribe(res=>{
      this.service.swal(res.title,res.message,res.icon);
      if(res.code=="1") {
        this.hubConnection.invoke('refresh', 'costumer',this.costumer.idEmpresa,0,0);
        document.getElementById('btnCloseClient2').click();
        document.getElementById('btnCloseClient').click();

        this.ticket.idCliente = String(res.data.id);
        this.costumer = {};
      }
      this.service.isLoading =false;
      },error => {
        this.service.handleHttpError(error);
        this.service.isLoading =false;
      });
  }

  getParts(id,option){
    this.service.isLoading = true;
    this.service.get('Part/'+ id + '/' + option)
      .subscribe(res=>{
        this.parts = res;
        // console.log(  this.parts )
        this.service.isLoading = false;
      },error => {
        this.service.handleHttpError(error);
        this.service.isLoading = false;
      });
  }

  addPartOne(part){
    if(part.cantidad <1 || this.service.isNullorEmpty(part.noSerial) || this.service.isNullorEmpty(part.descripcion)
    || this.service.isNullorEmpty(part.cantidad) ){
      this.service.swal(this.service.messages.titleRequiredFields,this.service.messages.fillRequiredFields,'warning');
      return false;
    }
    this.service.isLoading = true;
    part.idSolicitud = this.ticket.id;
    part.idEmpresa = this.ticket.idEmpresa;
    // console.log('devices')
    // console.log(this.devices)
     this.service.post('Part', part)
      .subscribe(res=>{
      // console.log( res )
      
      if(res.code=="1") {
        // console.log('part saved')
        this.addPart = false;
        this.part = {};
        // this.getDevices(this.ticket.id,this.service.getUser().idEmpresa);
        this.hubConnection.invoke('refresh', 'ticket',this.ticket.idEmpresa,this.ticket.idUsuario,this.ticket.id===undefined?0:this.ticket.id)
      }else{
        this.parts = [];
        this.service.swal(res.title,res.message,res.icon);
      }
      this.service.isLoading =false;
      },error => {
        this.service.handleHttpError(error);
        this.service.isLoading =false;
      });
  }

  addDevicePostOne(device){
    this.service.isLoading = true;
    // console.log('devices')
    // console.log(this.devices)
     this.service.post('Device/PostOne', device)
      .subscribe(res=>{
      // console.log( res )
      
      if(res.code=="1") {
        // console.log('devices saved')
        // this.getDevices(this.ticket.id,this.service.getUser().idEmpresa);
        this.hubConnection.invoke('refresh', 'ticket',this.ticket.idEmpresa,this.ticket.idUsuario,this.ticket.id===undefined?0:this.ticket.id)
      }else{
        this.devices = [];
        this.service.swal(res.title,res.message,res.icon);
      }
      this.service.isLoading =false;
      },error => {
        this.service.handleHttpError(error);
        this.service.isLoading =false;
      });
  }

  addDeviceOne(item){
    if(this.service.validateTrim(item.marca) || this.service.validateTrim(item.fallaReportada)
    ||this.service.validateTrim(item.modelo) ||this.service.validateTrim(item.noSerial)){
      this.service.swal(this.service.messages.titleRequiredFields,this.service.messages.deviceDataRequired,'info');
      return false;
    }
    if(this.deviceForm.invalid){
      this.service.swal(this.service.messages.titleRequiredFields,this.service.messages.deviceDataTooLong,'info');
      return false;
    }
    item.id = 0;
    item.idEmpresa = this.service.getUser().idEmpresa;
    item.idSolicitud = Number(this.ticket.id);
    // this.devices.push(item)
    this.addDevicePostOne(item);
    this.addDevice =false;
  }

  parToEdit:any={};
  partChoosen(item){
    this.parToEdit = item;

  }

  editPartUsed(part){
    if(part.cantidad <1 || this.service.isNullorEmpty(part.noSerial) || this.service.isNullorEmpty(part.descripcion)
    || this.service.isNullorEmpty(part.cantidad) ){
      this.service.swal(this.service.messages.titleRequiredFields,this.service.messages.fillRequiredFields,'warning');
      return false;
    }
    this.service.isLoading = true;
    
    this.service.post('Part/Put', part)
    .subscribe(res=>{
    // console.log( res )
    
    if(res.code=="1") {
      // console.log('part edited')
      this.addPart = false;
      this.part = {};
      this.hubConnection.invoke('refresh', 'ticket',this.ticket.idEmpresa,this.ticket.idUsuario,this.ticket.id===undefined?0:this.ticket.id)
      this.service.swal(res.title,res.message,res.icon);
    }else{
      this.getParts(this.ticket.id,this.service.getUser().idEmpresa);
      this.service.swal(res.title,res.message,res.icon);
    }
    this.service.isLoading =false;
    },error => {
      this.service.handleHttpError(error);
      this.service.isLoading =false;
    });
  }

  deletePartUsed(part){
    // console.log(part);
    this.service.post('Part/Delete/'+part.id)
    .subscribe(res=>{
    // console.log( res )
    
    if(res.code=="1") {
      // console.log('part deleted')
      this.addPart = false;
      this.part = {};
      // this.getDevices(this.ticket.id,this.service.getUser().idEmpresa);
      this.hubConnection.invoke('refresh', 'ticket',this.ticket.idEmpresa,this.ticket.idUsuario,this.ticket.id===undefined?0:this.ticket.id)
      this.service.swal(res.title,res.message,res.icon);
    }else{
      this.getParts(this.ticket.id,this.service.getUser().idEmpresa);
      this.service.swal(res.title,res.message,res.icon);
    }
    this.service.isLoading =false;
    },error => {
      this.service.handleHttpError(error);
      this.service.isLoading =false;
    });
  }

  deviceToEdit:any={};
  deviceChoosen(item){
    // console.log(item);
    this.deviceToEdit = item;
  }

  deleteDevice(part){
    // console.log(part);
    this.service.post('Device/Delete/'+part.id)
    .subscribe(res=>{
    // console.log( res )
    
    if(res.code=="1") {
      // console.log('device deleted')
      this.addPart = false;
      this.part = {};
      // this.getDevices(this.ticket.id,this.service.getUser().idEmpresa);
      this.hubConnection.invoke('refresh', 'ticket',this.ticket.idEmpresa,this.ticket.idUsuario,this.ticket.id===undefined?0:this.ticket.id)
      this.service.swal(res.title,res.message,res.icon);
    }else{
      this.getDevices(this.ticket.id);
      this.service.swal(res.title,res.message,res.icon);
    }
    this.service.isLoading =false;
    },error => {
      this.service.handleHttpError(error);
      this.service.isLoading =false;
    });
  }

  editDevice(part){
    if(this.service.validateTrim(part.marca) || this.service.validateTrim(part.fallaReportada)
    ||this.service.validateTrim(part.modelo) ||this.service.validateTrim(part.noSerial)){
      this.service.swal(this.service.messages.titleRequiredFields,this.service.messages.deviceDataRequired,'info');
      return false;
    }
    this.service.isLoading = true;
    
    this.service.post('Device/Put' + (this.mode == 'inbox' ? '/' : ''), part)
    .subscribe(res=>{
    // console.log( res )
    
    if(res.code=="1") {
      // console.log('part edited')
      this.addPart = false;
      this.part = {};
      this.hubConnection.invoke('refresh', 'ticket',this.ticket.idEmpresa,this.ticket.idUsuario,this.ticket.id===undefined?0:this.ticket.id)
      this.service.swal(res.title,res.message,res.icon);
    }else{
      this.getParts(this.ticket.id,this.service.getUser().idEmpresa);
      this.service.swal(res.title,res.message,res.icon);
    }
    this.service.isLoading =false;
    },error => {
      this.service.handleHttpError(error);
      this.service.isLoading =false;
    });
  }
}
