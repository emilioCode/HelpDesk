import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../../../services/api.service';
// import * as signalR from '@aspnet/signalr';
import * as signalR from '@microsoft/signalr';

@Component({
  selector: 'app-empresa',
  templateUrl: './empresa.component.html',
  styles: []
})
export class EmpresaComponent implements OnInit {

  constructor(private service: ApiService) { 
    if(this.service.getLevel(this.service.getUser().acceso) < 3 ){
      alert("No tiene permisos para acceder");
      this.service.route.navigateByUrl('/');
    }
    this.getBusiness(this.service.getUser().id);
  }

  isLoading = false;
  empresas:any;
  option;
  empresa:any={};
  imageUrl;
  hubConnection: signalR.HubConnection;

  ngOnInit() {
    this.hubConnection = new signalR.HubConnectionBuilder()
    .withUrl('/hub'//this.service.baseUrl+'/hub'
    ,{
      skipNegotiation: true,
      transport: signalR.HttpTransportType.WebSockets, // | signalR.HttpTransportType.LongPolling
    })
    .configureLogging(signalR.LogLevel.Debug)
    .build();

    this.hubConnection.on('refresh', (component, idEmpresa,idUsuario,idOther) => {
      // console.log(`component: ${component} | idEmpresa: ${idEmpresa} | idUsuario: ${idUsuario} | idOther: ${idOther}`)
      // debugger
      if( (component=='empresa' && idEmpresa == this.service.getUser().idEmpresa) || this.service.getUser().acceso =="ROOT" ){
        
        /* */
        // this.service.isLoading = true;
        this.service.http.get(this.service.baseUrl + 'api/Business/'+this.service.getUser().id,{headers:this.service.headers,responseType:'json'})
          .subscribe(res=>{
            this.empresas = res;
            var item =this.empresas.filter(e=>e.id == this.service.getUser().idEmpresa)[0];
            if(item.habilitado != true){
              this.service.closeSession();
            }else{
              this.fillModal('edit',item)
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

  getBusiness(id){
    this.service.isLoading = true;
    this.service.http.get(this.service.baseUrl + 'api/Business/'+id,{headers:this.service.headers,responseType:'json'})
      .subscribe(res=>{
        this.empresas = res;
        this.service.isLoading = false;
      },error => {
        console.error(error);
        this.service.isLoading = false;
      });
  }

  fillModal(option='add',object){
    this.option = option;
    this.empresa = object;
    // console.log(this.empresa)
  }

  fileUpload:File = null;
  handleFileInput(file: FileList){
    // console.log(file)
    this.fileUpload = file.item(0);
    // console.log(this.fileUpload)
    //Show image preview
    var reader = new FileReader();
    reader.onload = (event:any) =>{
      this.imageUrl = event.target.result;
      // debugger;
      // console.log(this.imageUrl);
      this.empresa.image = this.imageUrl.split(',')[1];
      
    }
    reader.readAsDataURL(this.fileUpload);
  }

  add(){
    this.service.isLoading = true;
     this.service.http.post(this.service.baseUrl + 'api/Business',this.empresa,{headers:this.service.headers,responseType:'json'})
      .subscribe(res=>{
      // console.log( res )
      this.service.swal(res.title,res.message,res.icon);
      if(res.code=="1") {
        this.getBusiness(this.service.getUser().id);
      }
      this.service.isLoading =false;
      },error => {
        console.error(error);
        this.service.isLoading =false;
      });
  }

  edit(){
    this.service.isLoading = true;
    this.empresa.habilitado = true;
     this.service.http.post(this.service.baseUrl + 'api/Business/Put/'+this.empresa.id,this.empresa,{headers:this.service.headers,responseType:'json'})
      .subscribe(res=>{
      this.service.swal(res.title,res.message,res.icon);
      if(res.code=="1") {
        // this.getBusiness(this.service.getUser().id);
        this.hubConnection.invoke('refresh', 'empresa', this.empresa.id, 0,0)
      }
      this.service.isLoading =false;
      },error => {
        console.error(error);
        this.service.isLoading =false;
      });
  }
  delete(){
    this.service.isLoading = true;
    this.empresa.habilitado = !this.empresa.habilitado;
     this.service.http.post(this.service.baseUrl + 'api/Business/Put/'+this.empresa.id,this.empresa,{headers:this.service.headers,responseType:'json'})
      .subscribe(res=>{
      this.service.swal(res.title,res.message,res.icon);
      if(res.code=="1") {
        // this.getBusiness(this.service.getUser().id);        
        this.hubConnection.invoke('refresh', 'empresa', this.empresa.id, 0,0)
      }
      this.service.isLoading =false;
      },error => {
        console.error(error);
        this.service.isLoading =false;
      });

  }

}
