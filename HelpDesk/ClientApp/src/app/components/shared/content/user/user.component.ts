import { Component, OnInit, Pipe } from '@angular/core';
import { ApiService } from '../../../../services/api.service';
import { FilterPipe } from '../../../../pipes/filter.pipe';
// import * as signalR from '@aspnet/signalr';
import * as signalR from '@microsoft/signalr';
@Component({
  selector: 'app-user',
  templateUrl: './user.component.html',
  styles: []
})
@Pipe({name:'FilterPipe'})
export class UserComponent implements OnInit {
  
  constructor(private service:ApiService) {
    if(this.service.getLevel(this.service.getUser().acceso) < 3 ){
      alert("No tiene permisos para acceder");
      this.service.route.navigateByUrl('/');
    }else{
      this.getUsers(this.service.getUser().id,"*");
    }
    
  }

  isLoading = false;
  empresas:any;
  users:any;
  option;
  user:any={};
  imageUrl;
  levels:any;
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
      if( (component=='users' && idEmpresa == this.service.getUser().idEmpresa) ){
        
        /* */
        // this.service.isLoading = true;
        this.service.http.get(this.service.baseUrl + 'api/User/'+ this.service.getUser().id + '/' + '*',{headers:this.service.headers,responseType:'json'})
          .subscribe(res=>{
            this.users = res;
            this.service.updateSession(this.users);
            if(!this.service.getUser().habilitado){
              alert('Su usuario ha sido deshabilitado, comuniquese con el administrador');
              this.service.closeSession();
            }
          
            this.service.isLoading = false;
          },error => {
            console.error(error);
            this.service.isLoading = false;
          });
        /* */

      }
      
    })

    this.hubConnection.start().catch(err => console.error(err.toString()));
  }

  renderHTML1:string='';
  renderHTML2:string='';
  renderHTML3:string='';
  message:string='';

  getUsers(id,option){
    this.service.isLoading = true;
    this.service.http.get(this.service.baseUrl + 'api/User/'+ id + '/' + option,{headers:this.service.headers,responseType:'json'})
      .subscribe(res=>{
        this.users = res;
        this.service.updateSession(this.users);
        this.service.isLoading = false;
      },error => {
        console.error(error);
        this.service.isLoading = false;
      });
  }

  fillModal(option='add',object){
    this.option = option;
    this.user = object;
    if(option=='add')this.user.idEmpresa = this.service.getUser().idEmpresa;
    // console.log(this.user)
    this.getBusiness(this.service.getUser().id);
    this.levels = this.service.levels.filter(l=>l.value <= this.service.getLevel(this.service.getUser().acceso));

    this.renderHTML1='';
    this.renderHTML2='';
    this.renderHTML3='';
    this.message='';
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
      this.user.image = this.imageUrl.split(',')[1];
      
    }
    reader.readAsDataURL(this.fileUpload);
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

  add(){
    this.service.isLoading = true;
     this.service.http.post(this.service.baseUrl + 'api/User',this.user,{headers:this.service.headers,responseType:'json'})
      .subscribe(res=>{
      // console.log( res )
      this.service.swal(res.title,res.message,res.icon);
      if(res.code=="1") {
        // this.getUsers(this.service.getUser().id,"*");
        this.hubConnection.invoke('refresh', 'users',this.user.idEmpresa,this.user.id,0)
      }
      this.service.isLoading =false;
      },error => {
        console.error(error);
        this.service.isLoading =false;
      });
  }

  edit(){
    this.service.isLoading = true;
    this.user.habilitado = true;
     this.service.http.post(this.service.baseUrl + 'api/User/Put/'+this.service.getUser().id,this.user,{headers:this.service.headers,responseType:'json'})
      .subscribe(res=>{
      this.service.swal(res.title,res.message,res.icon);
      if(res.code=="1") {
        // this.getUsers(this.service.getUser().id,"*");
        this.hubConnection.invoke('refresh', 'users',this.user.idEmpresa,this.user.id,0)
      }
      this.service.isLoading =false;
      },error => {
        console.error(error);
        this.service.isLoading =false;
      });
  }
  delete(){
    this.service.isLoading = true;
    this.user.habilitado = !this.user.habilitado;
     this.service.http.post(this.service.baseUrl + 'api/User/Put/'+this.user.id,this.user,{headers:this.service.headers,responseType:'json'})
      .subscribe(res=>{
      this.service.swal(res.title,res.message,res.icon);
      if(res.code=="1") {
        // this.getUsers(this.service.getUser().id,"*");
        this.hubConnection.invoke('refresh', 'users',this.user.idEmpresa,this.user.id,0)
        var abled = this.user.habilitado== true?1:0;
        this.hubConnection.invoke('refresh', 'session',this.user.idEmpresa,this.user.id,abled)
      }
      this.service.isLoading =false;
      },error => {
        console.error(error);
        this.service.isLoading =false;
      });

  }
  


  validateUserAccount(userAccount){
    this.service.isLoading = true;
    this.service.http.get(this.service.baseUrl + 'api/Login/'+ this.service.getUser().idEmpresa + '/' + userAccount,{headers:this.service.headers,responseType:'json'})
      .subscribe(res=>{
        
        this.renderHTML1= res.data.renderHTML1;
        this.renderHTML2= res.data.renderHTML2;
        this.renderHTML3= res.data.renderHTML3;
        this.message = res.message;
        

        this.service.isLoading = false;
      },error => {
        console.error(error);
        this.service.isLoading = false;
      });
  }
}
