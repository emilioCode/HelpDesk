import { Component, OnDestroy, OnInit, Pipe } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ApiService } from '../../../../services/api.service';
// import * as signalR from '@aspnet/signalr';
import * as signalR from '@microsoft/signalr';

@Component({
  selector: 'app-costumer',
  templateUrl: './costumer.component.html',
  styleUrls: [],
  styles: ['.form-control.ng-invalid.ng-touched { border-color: #dd4b39; }']
})
@Pipe({name:'FilterPipe'})
export class CostumerComponent implements OnInit, OnDestroy {

  constructor(private service: ApiService, private formBuilder: FormBuilder) { 
    this.initializeForm();
    this.getCostumers(this.service.getUser().id,"*")
  }

  isLoading = false;
  empresas:any;
  costumers:any;
  option;
  costumer:any={};
  imageUrl;
  levels:any;
  hubConnection: signalR.HubConnection;
  form: FormGroup;

  // Limits are the ones of the database columns (and CustomerValidator for the name), so a long value shows before saving.
  initializeForm = (): void => {
    this.form = this.formBuilder.group({
      nombre: ['', [Validators.required, Validators.maxLength(100)]],
      rnc: ['', [Validators.required, Validators.maxLength(30)]],
      contacto: ['', Validators.maxLength(80)],
      telefono: ['', Validators.maxLength(20)],
      extension: ['', Validators.maxLength(10)],
      correo: ['', Validators.maxLength(50)],
      departamento: ['', Validators.maxLength(60)],
      idEmpresa: [null],
      direccion: ['']
    });
  }

  // The form holds what the person types; the rest of the customer (id, status...) stays in this.costumer.
  private payload() {
    return Object.assign({}, this.costumer, this.form.value);
  }

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
      console.log(`component: ${component} | idEmpresa: ${idEmpresa} | idUsuario: ${idUsuario} | idOther: ${idOther}`)
      // debugger
      if( (component=='costumer' && idEmpresa == this.service.getUser().idEmpresa) || this.service.getUser().acceso ==this.service.roles.root ){
        
        /* */
        this.service.get('Costumer/'+ this.service.getUser().id + '/' + '*')
          .subscribe(res=>{
            this.costumers = res;
            if(idUsuario >0 && this.costumer.id == idUsuario){
              this.costumer = this.costumers.filter(c=>c.id==idUsuario)[0];
            }
            this.service.isLoading = false;
          },error => {
            this.service.handleHttpError(error);
            this.service.isLoading = false;
          });
        /* */

      }
      
    })

    this.hubConnection.start().catch(err => console.error(err.toString()));
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

  fillModal(option='add',object){
    this.option = option;
    this.costumer = object;
    if(option=='add')this.costumer.idEmpresa = this.service.getUser().idEmpresa;
    this.form.reset();
    this.form.patchValue(object);
    // Same rules as before: nobody edits while disabling, a technician only reads, and only root changes the company.
    const level = this.service.getLevel(this.service.getUser().acceso);
    option == 'delete' || level <= 1 ? this.form.disable() : this.form.enable();
    if(level != 4) this.form.get('idEmpresa').disable();
    // console.log(this.user)
    this.getBusiness(this.service.getUser().id);
  }

  getBusiness(id){
    this.service.isLoading = true;
    this.service.get('Business/'+id)
      .subscribe(res=>{
        this.empresas = res;
        this.service.isLoading = false;
      },error => {
        this.service.handleHttpError(error);
        this.service.isLoading = false;
      });
  }

  add(){
    if(this.service.getLevel(this.service.getUser().acceso) <= 1){
      this.service.swal(this.service.messages.titleAccessDenied,'','error');
      return false;
    }
    this.service.isLoading = true;
     this.service.post('Costumer', this.payload())
      .subscribe(res=>{
      console.log( res )
      this.service.swal(res.title,res.message,res.icon);
      if(res.code=="1") {
        // this.getCostumers(this.service.getUser().id,"*")
        this.hubConnection.invoke('refresh', 'costumer',this.costumer.idEmpresa,0,0)
      }
      this.service.isLoading =false;
      },error => {
        this.service.handleHttpError(error);
        this.service.isLoading =false;
      });
  }

  edit(){
    if(this.service.getLevel(this.service.getUser().acceso) <= 1){
      this.service.swal(this.service.messages.titleAccessDenied,'','error');
      return false;
    }
    this.service.isLoading = true;
    this.costumer.habilitado = true;
     this.service.post('Costumer/Put/'+ this.service.getUser().id, this.payload())
      .subscribe(res=>{
      this.service.swal(res.title,res.message,res.icon);
      if(res.code=="1") {
        // this.getCostumers(this.service.getUser().id,"*")
        this.hubConnection.invoke('refresh', 'costumer',this.costumer.idEmpresa,this.costumer.id,0)
      }
      this.service.isLoading =false;
      },error => {
        var errors = error.error.errors;
        var title = error.error.title ? error.error.title : 'Warning';
        var list = [];
        if(title !== "Warning"){
          var objectKeys =  Object.keys(errors);
          objectKeys.forEach(x => {
            list.push(errors[x]);
          });
        }
        var message = title !== "Warning" ? list.join(','): errors[0].detail;
        this.service.swal(title, message, 'warning');
        console.error(error);
        this.service.isLoading =false;
      });
  }

  delete(){
    this.service.isLoading = true;
    this.costumer.habilitado = !this.costumer.habilitado;
     this.service.post('Costumer/Put/'+this.service.getUser().id, this.costumer)
      .subscribe(res=>{
      this.service.swal(res.title,res.message,res.icon);
      if(res.code=="1") {
        // this.getCostumers(this.service.getUser().id,"*")
        this.hubConnection.invoke('refresh', 'costumer',this.costumer.idEmpresa,0,0)
      }
      this.service.isLoading =false;
      },error => {
        var errors = error.error.errors;
        var title = error.error.title ? error.error.title : 'Warning';
        var list = [];
        if(title !== "Warning"){
          var objectKeys =  Object.keys(errors);
          objectKeys.forEach(x => {
            list.push(errors[x]);
          });
        }
        var message = title !== "Warning" ? list.join(','): errors[0].detail;
        this.service.swal(title, message, 'warning');
        console.error(error);
        this.service.isLoading =false;
      });

  }

}
