import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../../../services/api.service';

@Component({
  selector: 'app-empresa',
  templateUrl: './empresa.component.html',
  styles: []
})
export class EmpresaComponent implements OnInit {

  constructor(private service: ApiService) { 
    console.log('EmpresaComponent');
    this.getBusiness(this.service.getUser().id);
  }

  isLoading = false;
  empresas:any;
  ngOnInit() {
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

}
