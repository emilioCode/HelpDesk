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

  // Fixed values shared by the screens. Use these instead of writing the text in components or templates.
  readonly roles = {
    root: 'ROOT',
    admin: 'ADMINISTRADOR',
    moderator: 'MODERADOR',
    technician: 'TECNICO'
  };
  readonly ticketStatuses = {
    open: 'Abierto',
    inProgress: 'en Proceso',
    completed: 'Completado'
  };
  // Values of ticket.tipoSolicitud
  readonly requestTypes = {
    workshop: 'Servicio Taller',
    onSite: 'Servicio a Domicilio'
  };
  // Values of ticket.tipoServicio
  readonly serviceTypes = {
    warranty: 'Garantía',
    contract: 'Contrato',
    billable: 'Facturable',
    rental: 'Renta',
    loan: 'Préstamo',
    internal: 'Interno'
  };

  // Texts shown to the user by more than one screen.
  readonly messages = {
    // Titles
    titleRequiredFields: 'Campos requeridos',
    titleAccessDenied: 'Access denied',
    titleClosedOrder: 'Orden Cerrada',
    titleAdjustmentNotApplied: 'Ajuste no aplicado',
    titleRequiredActivities: 'Actividades requeridas',
    // Validation and permission notices
    fillRequiredFields: 'Favor completar campos.',
    enterReportedFailure: 'Favor digite falla reportada',
    completeMinimumOrderData: 'Asegúrese de completar los datos mínimos necesarios para crear la orden',
    deviceDataRequired: 'Es necesario suministrar los datos del dispositivo',
    mustHaveDevice: 'Must have a device as well',
    optionRequired: 'an option is required',
    activityRequired: 'Se necesita al menos una actividad realizada',
    noPermissionToAccess: 'No tiene permisos para acceder',
    noAdminPermissionToAdjust: 'No cuenta con los permisos de Administrador para realizar el ajuste',
    userDisabled: 'Su usuario ha sido deshabilitado, comuníquese con el administrador',
    ticketWithoutTechnician: 'Esta orden no tiene técnico asignado.\nNecesita primero asignar un técnico para trabajarla.',
    closedOrderAdjustment: 'No se pudo aplicar ajuste debido a que la orden se encuentra cerrada.',
    // Confirmations
    confirmReopenTicket: 'Seguro que desea reabrir este ticket?',
    confirmCompleteTicket: 'Seguro que desea confirmar como completado?',
    confirmAssignTicket: 'Seguro que desea asignar este ticket a este usuario?',
    usedPartsQuestion: 'Utilizó partes o repuestos?',
    addPartQuestion: 'Desea adicionar algún repuesto?',
    yes: 'Si',
    noOrderIsComplete: 'No, ya está completo',
    // Session
    titleSessionExpired: 'Sesión expirada',
    sessionExpired: 'Su sesión expiró. Inicie sesión de nuevo.',
    titleSessionExpiring: 'Su sesión está por expirar',
    sessionExpiring: 'Por seguridad, su sesión se cerrará pronto. ¿Desea continuar?',
    continueSession: 'Continuar'
  };

  // Shows the validation message returned by the API (HTTP 400) and stops the loading indicator.
  handleHttpError(error: any): void {
    console.error(error);
    this.isLoading = false;
    const body = error && error.error;
    const errors = body && body.errors;
    let title = 'Warning';
    let message = '';
    if (Array.isArray(errors) && errors.length > 0 && errors[0].detail) {
      message = errors[0].detail;
    } else if (errors && typeof errors === 'object') {
      message = Object.keys(errors).map(k => errors[k]).join(', ');
      title = body.title || title;
    }
    if (message) {
      this.swal(title, message, 'warning');
    }
  }
  isLoading:boolean;
  level;
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

  setUser(user:any):void{
    // The access token comes with the user. Its lifetime is kept so the renewal can be scheduled.
    if (user && user.expiresAt) {
      user.tokenLifetimeMs = new Date(user.expiresAt).getTime() - Date.now();
    }
    this.sessionSt.store('user',user);
    this.route.navigateByUrl('/');
    window.location.reload();
  }

  closeSession(){
    // Also clears the cookie the printable reports use. If it fails the cookie expires with the token.
    this.http.post(this.baseUrl + 'api/Login/Logout', {}).subscribe(() => {}, () => {});
    while (this.sessionSt.retrieve('user')!=null) {
      this.sessionSt.clear('user');
      this.route.navigateByUrl('/');
    }

  }

  getUser(){
    return this.sessionSt.retrieve('user');
  }

  getToken(): string {
    const user = this.getUser();
    return user && user.token ? user.token : null;
  }

  getTokenExpiry(): Date {
    const user = this.getUser();
    return user && user.expiresAt ? new Date(user.expiresAt) : null;
  }

  // Stores a renewed token and how long it lasts.
  setToken(token: string, expiresAt: string): void {
    const user = this.getUser();
    if (!user) {
      return;
    }
    user.token = token;
    user.expiresAt = expiresAt;
    user.tokenLifetimeMs = new Date(expiresAt).getTime() - Date.now();
    this.sessionSt.store('user', user);
  }

  // The API said the token is no longer valid: end the session once, however many calls fail at the same time.
  private sessionEnding = false;
  expireSession(): void {
    if (!this.getUser() || this.sessionEnding) {
      return;
    }
    this.sessionEnding = true;
    this.isLoading = false;
    this.closeSession();
    this.swal(this.messages.titleSessionExpired, this.messages.sessionExpired, 'info');
    this.sessionEnding = false;
  }

  //the Distinct from a List
  private  distinct  = (value,index,self)=>{
    return self.indexOf(value)===index;
  }

  getDistinct(array){
    return array.filter(this.distinct)
  }

  levels = [
    { name: this.roles.root, value:4},
    { name: this.roles.admin, value:3},
    { name: this.roles.moderator, value:2},
    { name: this.roles.technician, value:1}
  ];

  getLevel(acceso):number{
    this.level= this.levels.filter(x=>x.name==acceso)[0].value;
    return this.level;
  }

  updateSession(users:any):void{
    var session = users.filter(u=>u.id == this.getUser().id)[0];
    // Keep what the sign-in gave (the token and its expiry) when the profile data is refreshed.
    this.sessionSt.store('user', Object.assign({}, this.getUser(), session));
  }

  isNullorEmpty(value:any):boolean{
    return (value=='' || value==null)?true:false;
  }

  private ticketStatusList = [
    { name: this.ticketStatuses.open, value: this.ticketStatuses.open },
    { name: this.ticketStatuses.inProgress, value: this.ticketStatuses.inProgress },
    { name: this.ticketStatuses.completed, value: this.ticketStatuses.completed }
  ];

  getStatuses(){
    return this.ticketStatusList;
  }

  private requestTypeList = [
    { name: this.requestTypes.workshop, value: this.requestTypes.workshop },
    { name: this.requestTypes.onSite, value: this.requestTypes.onSite }
  ];

  private serviceTypeList = [
    { name: this.serviceTypes.warranty, value: this.serviceTypes.warranty },
    { name: this.serviceTypes.contract, value: this.serviceTypes.contract },
    { name: this.serviceTypes.billable, value: this.serviceTypes.billable },
    { name: this.serviceTypes.rental, value: this.serviceTypes.rental },
    { name: this.serviceTypes.loan, value: this.serviceTypes.loan },
    { name: this.serviceTypes.internal, value: this.serviceTypes.internal }
  ];

  // Options for ticket.tipoServicio
  getTypeOfServices(){
    return this.serviceTypeList;
  }

  // Options for ticket.tipoSolicitud
  getTypeOfRequests(){
    return this.requestTypeList;
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
