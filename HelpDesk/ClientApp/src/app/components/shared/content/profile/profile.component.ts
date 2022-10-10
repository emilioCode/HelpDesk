import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../../../services/api.service';
// import * as signalR from '@aspnet/signalr';
import * as signalR from '@microsoft/signalr';

@Component({
  selector: 'app-profile',
  templateUrl: './profile.component.html',
  styles: []
})
export class ProfileComponent implements OnInit {

  userT:any={};
  users;
  empresas;
  pwdValidation:string;
  imageBusiness;
  numbers:any={};
  hubConnection: signalR.HubConnection;

  ngOnInit() {
    this.hubConnection = new signalR.HubConnectionBuilder()
    .withUrl(this.service.baseUrl+'hub')
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

    if( component=='session' && idEmpresa == this.service.getUser().idEmpresa && idUsuario == this.service.getUser().id  ){
      if(idOther == 0 ){
        alert('Su usuario ha sido deshabilitado, comuniquese con el administrador');
        this.service.closeSession();
      }

    }
    })

    this.hubConnection.start().catch(err => console.error(err.toString()));
  }


  constructor(private service: ApiService) {4
    this.getUsers(this.service.getUser().id,'UNIQUE');
    this.numbersOfTickets(this.service.getUser().id);
    
  }

  getBusiness(id){
    this.service.isLoading = true;
    this.service.http.get(this.service.baseUrl + 'api/Business/'+id,{headers:this.service.headers,responseType:'json'})
      .subscribe(res=>{
        this.empresas = res;
        this.imageBusiness = this.empresas.filter(x=>x.id==this.service.getUser().idEmpresa)[0].image;
        this.service.isLoading = false;

      },error => {
        console.error(error);
        this.service.isLoading = false;
      });
  }

  getUsers(id,option){
    this.service.isLoading = true;
    this.getBusiness(this.service.getUser().id);
    this.service.http.get(this.service.baseUrl + 'api/User/'+ id + '/' + option,{headers:this.service.headers,responseType:'json'})
      .subscribe(res=>{
        this.users = res;
        this.userT = this.users[0];
        this.userT.contrasena = null;
        this.pwdValidation=null;
        // console.log(this.userT)
        this.service.updateSession(this.users);
        this.service.isLoading = false;
      },error => {
        console.error(error);
        this.service.isLoading = false;
      });
  }

  edit(){
    if(this.pwdValidation == '' || this.pwdValidation == null
    || this.userT.contrasena == '' || this.userT.contrasena == null
    ){
      this.service.swal('Campos requeridos','Se necesita rellenar los campo para optar por el cambio de contraseña','info');
      // return false;
    }else if(this.pwdValidation !== this.userT.contrasena){
      this.service.swal('Contraseñas no coinciden','La contraseña no coincide con su confirmación.\n Favor intentar nuevamente.','warning');
      // return false;
    }else{
    this.service.isLoading = true;
    // this.user.habilitado = true;
     this.service.http.post(this.service.baseUrl + 'api/User/Put/'+this.service.getUser().id,this.userT,{headers:this.service.headers,responseType:'json'})
      .subscribe(res=>{
      this.service.swal(res.title,res.message,res.icon);
      if(res.code=="1") {
        this.getUsers(this.service.getUser().id,"UNIQUE");
      }
      this.service.isLoading =false;
      },error => {
        console.error(error);
        this.service.isLoading =false;
      });
    }
  }

  numbersOfTickets(idUser){
    this.service.isLoading = true;
    this.service.http.get(this.service.baseUrl + 'api/Ticket/numbersOfTickets/'+ idUser ,{headers:this.service.headers,responseType:'json'})
      .subscribe(res=>{
        this.numbers =  res.data;
        // console.log( this.numbers );
        this.service.isLoading = false;
      },error => {
        console.error(error);
        this.service.isLoading = false;
      });
  }
}
