import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../../../services/api.service';

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
  constructor(private service: ApiService) {4
    this.getUsers(this.service.getUser().id,'UNIQUE');
    this.numbersOfTickets(this.service.getUser().id);
    
  }

  ngOnInit() {
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
     this.service.http.put(this.service.baseUrl + 'api/User/'+this.service.getUser().id,this.userT,{headers:this.service.headers,responseType:'json'})
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
        console.log( this.numbers );
      },error => {
        console.error(error);
        this.service.isLoading = false;
      });
  }
}
