import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../../../services/api.service';
// import * as signalR from '@aspnet/signalr';
import * as signalR from '@microsoft/signalr';

@Component({
  selector: 'app-report',
  templateUrl: './report.component.html',
  styles: []
})
export class ReportComponent implements OnInit {

  constructor(private service: ApiService) {
    if(this.service.getLevel(this.service.getUser().acceso) < 3 ){
      alert("No tiene permisos para acceder");
      this.service.route.navigateByUrl('/');
    }else{
      this.clear()
    }
    
   }

  requests:any=[];
  request:any={};

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

    if( component=='session' && idEmpresa == this.service.getUser().idEmpresa && idUsuario == this.service.getUser().id  ){
      if(idOther == 0 ){
        alert('Su usuario ha sido deshabilitado, comuniquese con el administrador');
        this.service.closeSession();
      }

    }
    })

    this.hubConnection.start().catch(err => console.error(err.toString()));
  }

  toSearch(){
  

    if(this.service.validateTrim(this.request.tipoSolicitud) && this.service.validateTrim(this.request.fechaInicio)
    && this.service.validateTrim(this.request.fechaTermino)){
      this.service.swal('Campos requeridos','Favor suministrar los datos para proceder con la consulta.','info');
      return false;
    } 
    else if(this.service.validateTrim(this.request.tipoSolicitud)  && (this.service.validateTrim(this.request.fechaInicio)
    || this.service.validateTrim(this.request.fechaTermino)  ) ){
      this.service.swal('Campos requeridos','Favor suministrar las fechas para proceder con la consulta.','warning');
      return false;
    }

    this.service.isLoading = true;
    this.service.http.post(this.service.baseUrl + 'api/Ticket/GetJsonTicket',this.request,{headers:this.service.headers,responseType:'json'})
      .subscribe(res=>{
        this.requests = res;
        // console.table( this.requests )
        this.requests.forEach(element => {
          if(element.horaInicio != null)element.horaInicio = element.horaInicio; //this.service.fillZeroWithNumber(element.horaInicio/*.value.hours*/) + ":" + this.service.fillZeroWithNumber(element.horaInicio.value.minutes) + ":" + this.service.fillZeroWithNumber(element.horaInicio.value.seconds); 
          if(element.horaTermino != null)element.horaTermino = element.horaTermino; //this.service.fillZeroWithNumber(element.horaTermino/*.value.hours*/) + ":" + this.service.fillZeroWithNumber(element.horaTermino.value.minutes) + ":" + this.service.fillZeroWithNumber(element.horaTermino.value.seconds); 
        });
        this.service.isLoading = false;
      },error => {
        console.error(error);
        this.service.isLoading = false;
      });
  }

  clear(){
    this.request = {};
    this.request.tipoSolicitud = '';
    this.request.idEmpresa = this.service.getUser().idEmpresa;
  }

  exportCSV(){
    
    var comma =",";
    var enter = "\n";
    var file = "NO. SECUENCIA, TIPO SOLICITUD, TIPO SERVICIO, ESTADO, ATENDIDO POR, CLIENTE, FECHA CREACION, FECHA INICIAL, HORA INICIAL, FECHA TERMINO, HORA TERMINO, APROBADO POR" + enter;
 
    this.requests.forEach(e => {
      var fechaCreacion = this.service.isNull(e.fechaCreacion) !=""?this.service.isNull(e.fechaCreacion).split('T')[0]:"";
      var fechaInicial = this.service.isNull(e.fechaInicio) !=""?this.service.isNull(e.fechaInicio).split('T')[0]:"";
      var fechaFinal = this.service.isNull(e.fechaTermino) !=""?this.service.isNull(e.fechaTermino).split('T')[0]:"";
      var aprobadorPor = this.service.isNull(e.aprobadoPor) != ""?this.service.isNull(this.service.replaceAll(e.aprobadoPor,',',' ')):"";
      var horaInicio = this.service.isNull(e.horaInicio) !=""?this.service.setTime(this.service.isNull(e.horaInicio)):"";
      var horaTermino = this.service.isNull(e.horaTermino) !=""?this.service.setTime(this.service.isNull(e.horaTermino)):"";
      var atendidoPor = this.service.isNull(e.atendidoPor) !=""?this.service.isNull(this.service.replaceAll(e.atendidoPor,',',' ')):"";
      file += this.service.setEspecialPasted(e.noSecuencia) + comma + this.service.isNull(e.tipoSolicitud) + comma + this.service.isNull(e.tipoServicio) + comma + this.service.isNull(e.estado) + comma + atendidoPor + comma + this.service.isNull(this.service.replaceAll(e.cliente,',',' ')) + comma + fechaCreacion + comma + fechaInicial + comma + horaInicio + comma + fechaFinal + comma + horaTermino + comma + aprobadorPor + enter;
    });
   
    var universalBOM = "\uFEFF";
    var link = document.createElement('a');
    link.href = "data:text/csv; charset=utf-8," + encodeURIComponent(universalBOM+file);
    link.download = "reporte solicitudes " + new Date + ".csv";
    link.click();
  }
}
