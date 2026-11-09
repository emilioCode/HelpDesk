import { Component, OnDestroy, OnInit, Pipe } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ApiService } from '../../../../services/api.service';
import { FilterPipe } from '../../../../pipes/filter.pipe';
// import * as signalR from '@aspnet/signalr';
import * as signalR from '@microsoft/signalr';
@Component({
  selector: 'app-user',
  templateUrl: './user.component.html',
  styles: ['.form-control.ng-invalid.ng-touched { border-color: #dd4b39; }']
})
@Pipe({name:'FilterPipe'})
export class UserComponent implements OnInit, OnDestroy {
  
  constructor(private service:ApiService, private formBuilder: FormBuilder) {
    this.initializeForm();
    if(this.service.getLevel(this.service.getUser().acceso) < 3 ){
      alert(this.service.messages.noPermissionToAccess);
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
  form: FormGroup;

  // Same rules as the server-side validator (UserValidator). The password is required only when creating (see fillModal).
  initializeForm = (): void => {
    this.form = this.formBuilder.group({
      nombre: ['', [Validators.required, Validators.maxLength(255)]],
      numDocumento: ['', [Validators.required, Validators.maxLength(30)]],
      cuentaUsuario: ['', [Validators.required, Validators.maxLength(20)]],
      contrasena: ['', Validators.minLength(6)],
      correo: ['', [Validators.maxLength(50), Validators.email]],
      idEmpresa: [null, Validators.required],
      acceso: ['', [Validators.required, Validators.maxLength(20)]]
    });
  }

  // The form holds what the person types; the rest of the user (id, photo, status) stays in this.user.
  private payload() {
    return Object.assign({}, this.user, this.form.value);
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
      // console.log(`component: ${component} | idEmpresa: ${idEmpresa} | idUsuario: ${idUsuario} | idOther: ${idOther}`)
      // debugger
      if( (component=='users' && idEmpresa == this.service.getUser().idEmpresa) ){
        
        /* */
        // this.service.isLoading = true;
        this.service.get('User/'+ this.service.getUser().id + '/' + '*')
          .subscribe(res=>{
            this.users = res;
            this.service.updateSession(this.users);
            if(!this.service.getUser().habilitado){
              alert(this.service.messages.userDisabled);
              this.service.closeSession();
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

  renderHTML1:string='';
  renderHTML2:string='';
  renderHTML3:string='';
  message:string='';

  getUsers(id,option){
    this.service.isLoading = true;
    this.service.get('User/'+ id + '/' + option)
      .subscribe(res=>{
        this.users = res;
        this.service.updateSession(this.users);
        this.service.isLoading = false;
      },error => {
        this.service.handleHttpError(error);
        this.service.isLoading = false;
      });
  }

  fillModal(option='add',object){
    this.option = option;
    this.user = object;
    if(option=='add')this.user.idEmpresa = this.service.getUser().idEmpresa;
    this.form.reset();
    this.form.patchValue(object);
    const password = this.form.get('contrasena');
    password.setValidators(option == 'add' ? [Validators.required, Validators.minLength(6)] : [Validators.minLength(6)]);
    password.updateValueAndValidity();
    // The account name can be changed only by root once the user exists; nothing is edited when disabling.
    option == 'delete' ? this.form.disable() : this.form.enable();
    if(this.service.getUser().acceso != this.service.roles.root && option != 'add') this.form.get('cuentaUsuario').disable();
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
    this.service.isLoading = true;
     this.service.post('User', this.payload())
      .subscribe(res=>{
      this.service.swal(res.title,res.message,res.icon);
      if(res.code=="1") {
        this.hubConnection.invoke('refresh', 'users',this.user.idEmpresa,this.user.id,0)
      }
      document.getElementById('closeModalBtn').click();
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

  edit(){
    this.service.isLoading = true;
    this.user.habilitado = true;
     this.service.post('User/Put/'+this.service.getUser().id, this.payload())
      .subscribe(res=>{
      this.service.swal(res.title,res.message,res.icon);
      if(res.code=="1") {
        // this.getUsers(this.service.getUser().id,"*");
        this.hubConnection.invoke('refresh', 'users',this.user.idEmpresa,this.user.id,0)
      }
      document.getElementById('closeModalBtn').click();
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
    this.user.habilitado = !this.user.habilitado;
     this.service.post('User/Put/'+this.user.id, this.user)
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
        this.service.handleHttpError(error);
        this.service.isLoading =false;
      });

  }
  


  validateUserAccount(userAccount){
    this.service.isLoading = true;

    if(userAccount === null || userAccount == ''){
      this.renderHTML1 = "";
      this.renderHTML2 = "";
      this.renderHTML3 = "";
      this.message = "";
    }else{
      this.service.get('Login/'+ this.service.getUser().idEmpresa + '/' + userAccount)
      .subscribe(res=>{
        
        this.renderHTML1= res ? "has-success has-feedback": "has-warning has-feedback";
        this.renderHTML2= res ? "glyphicon glyphicon-ok form-control-feedback": "glyphicon glyphicon-warning-sign form-control-feedback";
        this.renderHTML3= res ? "text-success": "text-warning";
        this.message = res ? "It's ok.": "There exists another account with this name";
        

        this.service.isLoading = false;
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
}
