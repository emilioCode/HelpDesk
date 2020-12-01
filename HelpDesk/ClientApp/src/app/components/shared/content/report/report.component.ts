import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../../../services/api.service';

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

  ngOnInit() {
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
        console.table( this.requests )
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
    var file = "NO. SECUENCIA, TIPO SOLICITUD, TIPO SERVICIO, TITULO, ESTADO, ATENDIDO POR, CLIENTE, FECHA CREACION, FECHA INICIAL, HORA INICIAL, FECHA TERMINO, HORA TERMINO, APROBADO POR" + enter;
 
    this.requests.forEach(e => {
      var fechaCreacion = this.service.isNull(e.fechaCreacion) !=""?this.service.isNull(e.fechaCreacion).split('T')[0]:"";
      var fechaInicial = this.service.isNull(e.fechaInicio) !=""?this.service.isNull(e.fechaInicio).split('T')[0]:"";
      var fechaFinal = this.service.isNull(e.fechaTermino) !=""?this.service.isNull(e.fechaTermino).split('T')[0]:"";
      var aprobadorPor = this.service.isNull(e.aprobadoPor) != ""?this.service.isNull(this.service.replaceAll(e.aprobadoPor,',',' ')):"";
      var horaInicio = this.service.isNull(e.horaInicio) !=""?this.service.setTime(this.service.isNull(e.horaInicio)):"";
      var horaTermino = this.service.isNull(e.horaTermino) !=""?this.service.setTime(this.service.isNull(e.horaTermino)):"";
      file += this.service.setEspecialPasted(e.noSecuencia) + comma + this.service.isNull(e.tipoSolicitud) + comma + this.service.isNull(e.tipoServicio) + comma + this.service.isNull(this.service.replaceAll(e.titulo,',',' ')) + comma + this.service.isNull(e.estado) + comma + this.service.isNull(this.service.replaceAll(e.atendidoPor,',',' ')) + comma + this.service.isNull(this.service.replaceAll(e.cliente,',',' ')) + comma + fechaCreacion + comma + fechaInicial + comma + horaInicio + comma + fechaFinal + comma + horaTermino + comma + aprobadorPor + enter;
    });
   
    var universalBOM = "\uFEFF";
    var link = document.createElement('a');
    link.href = "data:text/csv; charset=utf-8," + encodeURIComponent(universalBOM+file);
    link.download = "reporte solicitudes " + new Date + ".csv";
    link.click();
  }
}
