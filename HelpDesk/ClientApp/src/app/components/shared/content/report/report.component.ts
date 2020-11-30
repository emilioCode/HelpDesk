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
        console.log( this.requests )
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
    console.log( 'downloading...' )
    this.requests.forEach(element => {
      
    });
  }
}
