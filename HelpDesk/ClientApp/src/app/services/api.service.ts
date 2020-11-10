import { Injectable,Inject } from '@angular/core';
import { SessionStorageService } from 'ngx-webstorage';

import { Router } from '@angular/router';
import { HttpClient, HttpHeaders } from '@angular/common/http';

import Swal from 'sweetalert2';

@Injectable()
export class ApiService {
  route;
  baseUrl;
  headers = new HttpHeaders().set("content-type", "application/json");
  http;
  swal = Swal;
  isLoading:boolean;
  constructor(
    private sessionSt: SessionStorageService,
    private _route: Router,
    @Inject('BASE_URL') _baseUrl: string,
    private _http:HttpClient
    ) {
      this.route =_route;
      this.baseUrl = _baseUrl; 
      this.http =_http;      
    console.log('ApiService is running')
  }

  //AplicationName
  aplicationName = 'HelpDesk';
  aplicationNameMini1 = 'H';
  aplicationNameMini2 = 'D';
  version = '1.0.0';
  year:any = new Date().getFullYear();;
  company = {
    name:'Company',
    value:'#'
  };
  copyright ='Copyright';
  legacy = 'Todo los derechos reservados.';
  info = '';

  user:any='';
  defaultPhoto = '../../../../assets/dist/img/photo_default.png';

  setUser(user:any):void{
    this.sessionSt.store('user',user);
    // sessionStorage.setItem('key', user);
    // let data = sessionStorage.getItem('key');
    // console.table(+data)
    this.route.navigateByUrl('/');
    window.location.reload();
  }

  closeSession(){
    // console.log( this.sessionSt.retrieve('user') )
    while (this.sessionSt.retrieve('user')!=null) {
      this.sessionSt.clear('user');
      this.route.navigateByUrl('/');
    }
    
  }

  getUser(){
    return this.sessionSt.retrieve('user');
  }

  //the Distinct from a List
  private  distinct  = (value,index,self)=>{
    return self.indexOf(value)===index;
  }

  getDistinct(array){
    return array.filter(this.distinct)
  }


  levels = [
    { name: 'ROOT', value:4},
    { name: 'ADMINISTRADOR', value:3},
    { name: 'MODERADOR', value:2},
    { name: 'TECNICO', value:1}
  ];

  level;
  
  getLevel(acceso):number{
    this.level= this.levels.filter(x=>x.name==acceso)[0].value;
    // console.log( this.level )
    return this.level;
  }

  updateSession(users:any):void{
    var session = users.filter(u=>u.id == this.getUser().id)[0];
    this.sessionSt.store('user',session);
  }

  isNullorEmpty(value:any):boolean{
    return (value=='' || value==null)?true:false;
  }




}
