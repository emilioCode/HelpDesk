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
  }

  //AplicationName
  aplicationName = 'HelpDesk';
  aplicationNameMini1 = 'H';
  aplicationNameMini2 = 'D';
  version = '1.0.0';
  year:any = new Date().getFullYear();;
  company = {
    name:'soporte de la aplicación',
    email:'mailto: emilio_mem@hotmail.com'
  };
  copyright ='Copyright';
  legacy = 'Todo los derechos reservados.';
  info = 'version: 2022.1';

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

  private ticketStatus = [
    { name: 'Abierto', value:'Abierto' },
    { name: 'en Proceso', value:'en Proceso' },
    { name: 'Completado', value:'Completado' }
  ];

  getStatuses(){
    return this.ticketStatus;
  }

  private  typeOfRequests=[
    { name: 'Garantía', value:'Garantía' },
    { name: 'Contrato', value:'Contrato' },
    { name: 'Facturable', value:'Facturable' },
    { name: 'Renta', value:'Renta' },
    { name: 'Préstamo', value:'Préstamo' },
    { name: 'Interno', value:'Interno' }
  ];

  private typeOfServices  =[
    { name: 'Servicio Taller', value:'Servicio Taller' },
    { name: 'Servicio a Domicilio', value:'Servicio a Domicilio' }
  ];

  getTypeOfServices(){
    return this.typeOfRequests;
  }

  getTypeOfRequests(){
    return this.typeOfServices;
  }

  validate(value): Boolean{
    
    return value == "" || value == null || value === undefined;
  }

  validateTrim(value):Boolean{
    return value == "" || value == null || value === undefined || value.trim() == "";
  }

  setTime(value1):String{
    var value = this.ftmHour(value1);
    var dateStr = value.split(':');
    var hora = Number(dateStr[0]) >12? Number(dateStr[0])-12 : Number(dateStr[0]);
    var tanda = Number(dateStr[0]) >12? " PM" : " AM";
    return ""+ hora +":"+ dateStr[1]+ tanda;

  }

  setEspecialPasted(str):string{
    return '=\"' + str + '\"';
  }

  isNull(str):string{
    return str != null? str:"";
  }

  replaceAll(text,oldValue,newValue):string{
    var str = "";
    for (var i = 0; i < text.length; i++) {
      str += text[i] == oldValue? newValue:text[i];
    }
    return str;
  }

  fillZeroWithNumber(n){
    return n > 9 ? "" + n: "0" + n;
  }

  ftmHour(timeSpan){
    return this.fillZeroWithNumber(timeSpan.hours) + ":" + this.fillZeroWithNumber(timeSpan.minutes) + ":" + this.fillZeroWithNumber(timeSpan.seconds);
  }
}
