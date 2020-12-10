import { Component, OnInit } from '@angular/core';
import { ApiService } from '../services/api.service';
import * as signalR from '@aspnet/signalr';
@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent implements OnInit{

  constructor(private service: ApiService){
    this.numbersOfTickets(this.service.getUser().id)
  }

  hubConnection: signalR.HubConnection;

  ngOnInit() {
    this.hubConnection = new signalR.HubConnectionBuilder()
    .withUrl(this.service.baseUrl+'/hub')
    .build();

    this.hubConnection.on('refresh', (component, idEmpresa,idUsuario,idOther) => {
      console.log(`component: ${component} | idEmpresa: ${idEmpresa} | idUsuario: ${idUsuario} | idOther: ${idOther}`)
      // debugger
      if( (component=='ticket' && idEmpresa == this.service.getUser().idEmpresa && idUsuario == this.service.getUser().id ) 
      || (this.service.getLevel(this.service.getUser().acceso)>1 && idEmpresa == this.service.getUser().idEmpresa)
     || (component=='customer' && idEmpresa == this.service.getUser().idEmpresa) ){
        
        /* */
        // this.service.isLoading = true;
        this.service.http.get(this.service.baseUrl + 'api/Ticket/numbersOfTickets/'+ this.service.getUser().id ,{headers:this.service.headers,responseType:'json'})
          .subscribe(res=>{
            this.numbers =  res.data;
            console.log( this.numbers );
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

  numbers:any = {};
  numbersOfTickets(idUser){
    this.service.isLoading = true;
    this.service.http.get(this.service.baseUrl + 'api/Ticket/numbersOfTickets/'+ idUser ,{headers:this.service.headers,responseType:'json'})
      .subscribe(res=>{
        this.numbers =  res.data;
        console.log( this.numbers );
        this.service.isLoading = false;
      },error => {
        console.error(error);
        this.service.isLoading = false;
      });
  }


}
