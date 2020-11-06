import { Component, OnInit, Pipe } from '@angular/core';
import { ApiService } from '../../../../services/api.service';

@Component({
  selector: 'app-costumer',
  templateUrl: './costumer.component.html',
  styleUrls: []
})
@Pipe({name:'FilterPipe'})
export class CostumerComponent implements OnInit {

  constructor(private service: ApiService) { 
    this.getCostumers(this.service.getUser().idEmpresa,"*")
  }

  ngOnInit() {
  }

  isLoading = false;
  empresas:any;
  costumers:any;
  option;
  costumer:any={};
  imageUrl;
  levels:any;

  getCostumers(id,option){
    this.service.isLoading = true;
    this.service.http.get(this.service.baseUrl + 'api/Costumer/'+ id + '/' + option,{headers:this.service.headers,responseType:'json'})
      .subscribe(res=>{
        this.costumers = res;
        this.service.isLoading = false;
      },error => {
        console.error(error);
        this.service.isLoading = false;
      });
  }

  fillModal(option='add',object){
    this.option = option;
    this.costumer = object;
    if(option=='add')this.costumer.idEmpresa = this.service.getUser().idEmpresa;
    // console.log(this.user)
    this.getBusiness(this.service.getUser().id);
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
