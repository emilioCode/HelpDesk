import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../../services/api.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {

  constructor(private service: ApiService, private formBuilder: FormBuilder) { }

  title = this.service.aplicationName;
  isLoading = false;
  form: FormGroup;

  ngOnInit() {
    this.initializeLoginForm();
  }

  initializeLoginForm = (): void => {
    this.form = this.formBuilder.group({
      user: ['', Validators.required],
      pwd: ['', Validators.required]
    });
  }

  singIn(){
    const values = this.form.value;
    this.isLoading =true;
    const obj = {
      CuentaUsuario: values.user,
      Contrasena: values.pwd
    }

    this.service.http.post(this.service.baseUrl + 'api/Login',obj,{headers:this.service.headers,responseType:'json'})
    .subscribe(res=>{
      if(res===null){
        this.service.swal('Acceso Denegado','Verifique el usuario o la contraseña, o sino comuniquese con soporte.','error');
      }else{
        this.service.setUser(res);
      }
      this.isLoading =false;
    },error => {
      let errors = error.error.errors;
      let title = error.error.title ? error.error.title : 'Warning';
      let list = [];
      if(title !== "Warning"){
        let objectKeys =  Object.keys(errors);
        objectKeys.forEach(x => {
          list.push(errors[x]);
        });
      }
      let message = title !== "Warning" ? list.join(','): errors[0].detail;
      this.service.swal(title, message, 'warning');
      console.error(error);
      this.service.isLoading =false;
    });
  }
}
