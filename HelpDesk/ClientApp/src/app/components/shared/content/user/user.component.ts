import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../../../services/api.service';

@Component({
  selector: 'app-user',
  templateUrl: './user.component.html',
  styles: []
})
export class UserComponent implements OnInit {

  constructor(private service:ApiService) {
    this.getUsers(this.service.getUser().id,"*");
  }

  isLoading = false;
  empresas:any;
  users:any;
  option;
  user:any={};
  imageUrl;
  levels:any;

  ngOnInit() {
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
    console.log(file)
    this.fileUpload = file.item(0);
    console.log(this.fileUpload)
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
      console.log( res )
      this.service.swal(res.title,res.message,res.icon);
      if(res.code=="1") {
        this.getUsers(this.service.getUser().id,"*");
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
     this.service.http.put(this.service.baseUrl + 'api/User/'+this.service.getUser().id,this.user,{headers:this.service.headers,responseType:'json'})
      .subscribe(res=>{
      this.service.swal(res.title,res.message,res.icon);
      if(res.code=="1") {
        this.getUsers(this.service.getUser().id,"*");
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
     this.service.http.put(this.service.baseUrl + 'api/User/'+this.user.id,this.user,{headers:this.service.headers,responseType:'json'})
      .subscribe(res=>{
      this.service.swal(res.title,res.message,res.icon);
      if(res.code=="1") {
        this.getUsers(this.service.getUser().id,"*");
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
