import { Component, OnInit } from '@angular/core';

import { ApiService } from '../../../services/api.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html'
})
export class LoginComponent implements OnInit {

  constructor(private service: ApiService) { }

  title = this.service.aplicationName;
  isLoading = false;

  user;
  pwd;
  ngOnInit() {
    // this.service.swal('Campos requeridos','Ambos campos son necesarios','warning');
  }

  singIn(){
    this.isLoading =true;
    var obj = {
      CuentaUsuario: this.user,
      Contrasena: this.pwd
    }
     if(this.user=='' ||this.pwd==''){
      this.service.swal('Campos requeridos','Ambos campos son necesarios','warning');
      this.isLoading =false;
      return false;
     }
     this.service.http.post(this.service.baseUrl + 'api/Login',obj,{headers:this.service.headers,responseType:'json'})
      .subscribe(res=>{
       if(res===null){
        this.service.swal('Acceso Denegado','Verifique el usuario o la contraseña, o sino comuniquese con soporte.','error');
        //this.user='';
        this.pwd='';
       }else{
        this.service.setUser(res);
        window.location.reload();
        // this.sessionSt.store('user',res);
        //console.log(this.service.getUser());
        // this.service.route.navigateByUrl()('/');
       }
        
       
       this.isLoading =false;
      },error => {
        console.error(error);
        this.isLoading =false;
      });
  }

}
