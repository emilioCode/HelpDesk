import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ApiService } from '../../../../services/api.service';
// import * as signalR from '@aspnet/signalr';
import * as signalR from '@microsoft/signalr';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styles: ['.form-control.ng-invalid.ng-touched { border-color: #dd4b39; }']
})
export class ProfileComponent implements OnInit, OnDestroy {

  userT:any={};
  users;
  empresas;
  form: FormGroup;

  // Only the password can be changed here: it needs 6 characters (as in UserValidator) and its confirmation.
  initializeForm = (): void => {
    this.form = this.formBuilder.group({
      idEmpresa: [null],
      cuentaUsuario: [''],
      correo: [''],
      contrasena: ['', [Validators.required, Validators.minLength(6)]],
      confirmacion: ['', Validators.required]
    }, { validator: (g: FormGroup) => g.get('contrasena').value === g.get('confirmacion').value ? null : { mismatch: true } });
  }

  // Shows the loaded user in the form. The password fields start empty.
  private fillForm() {
    this.form.reset();
    this.form.patchValue(this.userT);
    if(this.service.getLevel(this.service.getUser().acceso) != 4) this.form.get('idEmpresa').disable();
  }
  imageBusiness;
  numbers:any={};
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

    // Someone changed this user (name, e-mail...): show it here too, keeping what is being typed in the password fields.
    if( component=='users' && idEmpresa == this.service.getUser().idEmpresa && idUsuario == this.service.getUser().id ){
      this.refreshProfile();
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


  constructor(private service: ApiService, private formBuilder: FormBuilder) {
    this.initializeForm();
    this.getUsers(this.service.getUser().id,'UNIQUE');
    this.numbersOfTickets(this.service.getUser().id);
    
  }

  getBusiness(id){
    this.service.isLoading = true;
    this.service.get('Business/'+id)
      .subscribe(res=>{
        this.empresas = res;
        this.imageBusiness = this.empresas.filter(x=>x.id==this.service.getUser().idEmpresa)[0].image;
        this.service.isLoading = false;

      },error => {
        this.service.handleHttpError(error);
        this.service.isLoading = false;
      });
  }

  refreshProfile(){
    this.service.get('User/'+ this.service.getUser().id + '/UNIQUE')
      .subscribe((res:any)=>{
        this.users = res;
        const typed = { contrasena: this.form.get('contrasena').value, confirmacion: this.form.get('confirmacion').value };
        this.userT = this.users[0];
        this.userT.contrasena = null;
        this.fillForm();
        this.form.patchValue(typed);
        this.service.updateSession(this.users);
      },error => {
        this.service.handleHttpError(error);
      });
  }

  getUsers(id,option){
    this.service.isLoading = true;
    this.getBusiness(this.service.getUser().id);
    this.service.get('User/'+ id + '/' + option)
      .subscribe(res=>{
        this.users = res;
        this.userT = this.users[0];
        this.userT.contrasena = null;
        this.fillForm();
        // console.log(this.userT)
        this.service.updateSession(this.users);
        this.service.isLoading = false;
      },error => {
        this.service.handleHttpError(error);
        this.service.isLoading = false;
      });
  }

  edit(){
    if(this.form.invalid){
      this.service.swal(this.service.messages.titleRequiredFields,'Se necesita rellenar los campo para optar por el cambio de contraseña','info');
      return;
    }
    const values = this.form.value;
    const body = Object.assign({}, this.userT, { contrasena: values.contrasena });
    if(this.service.getLevel(this.service.getUser().acceso) == 4) body.idEmpresa = values.idEmpresa;
    this.service.isLoading = true;
     this.service.post('User/Put/'+this.service.getUser().id, body)
      .subscribe(res=>{
      this.service.swal(res.title,res.message,res.icon);
      if(res.code=="1") {
        this.getUsers(this.service.getUser().id,"UNIQUE");
      }
      this.service.isLoading =false;
      },error => {
        this.service.handleHttpError(error);
        this.service.isLoading =false;
      });
  }

  numbersOfTickets(idUser){
    this.service.isLoading = true;
    this.service.get('Ticket/numbersOfTickets/'+ idUser)
      .subscribe(res=>{
        this.numbers =  res.data;
        // console.log( this.numbers );
        this.service.isLoading = false;
      },error => {
        this.service.handleHttpError(error);
        this.service.isLoading = false;
      });
  }
}
