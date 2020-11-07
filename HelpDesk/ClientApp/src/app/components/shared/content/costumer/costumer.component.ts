import { Component, OnInit, Pipe } from '@angular/core';
import { ApiService } from '../../../../services/api.service';
import { serializePath } from '@angular/router/src/url_tree';

@Component({
  selector: 'app-costumer',
  templateUrl: './costumer.component.html',
  styleUrls: []
})
@Pipe({name:'FilterPipe'})
export class CostumerComponent implements OnInit {

  constructor(private service: ApiService) { 
    this.getCostumers(this.service.getUser().id,"*")
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

  add(){
    if(this.service.getLevel(this.service.getUser().acceso) <= 1){
      this.service.swal('Access denied','','error');
      return false;
    }
    this.service.isLoading = true;
     this.service.http.post(this.service.baseUrl + 'api/Costumer',this.costumer,{headers:this.service.headers,responseType:'json'})
      .subscribe(res=>{
      console.log( res )
      this.service.swal(res.title,res.message,res.icon);
      if(res.code=="1") {
        this.getCostumers(this.service.getUser().id,"*")
      }
      this.service.isLoading =false;
      },error => {
        console.error(error);
        this.service.isLoading =false;
      });
  }

  edit(){
    if(this.service.getLevel(this.service.getUser().acceso) <= 1){
      this.service.swal('Access denied','','error');
      return false;
    }
    this.service.isLoading = true;
    this.costumer.habilitado = true;
     this.service.http.put(this.service.baseUrl + 'api/Costumer/'+this.costumer.id,this.costumer,{headers:this.service.headers,responseType:'json'})
      .subscribe(res=>{
      this.service.swal(res.title,res.message,res.icon);
      if(res.code=="1") {
        this.getCostumers(this.service.getUser().id,"*")
      }
      this.service.isLoading =false;
      },error => {
        console.error(error);
        this.service.isLoading =false;
      });
  }

  delete(){
    this.service.isLoading = true;
    this.costumer.habilitado = !this.costumer.habilitado;
     this.service.http.put(this.service.baseUrl + 'api/Costumer/'+this.costumer.id,this.costumer,{headers:this.service.headers,responseType:'json'})
      .subscribe(res=>{
      this.service.swal(res.title,res.message,res.icon);
      if(res.code=="1") {
        this.getCostumers(this.service.getUser().id,"*")
      }
      this.service.isLoading =false;
      },error => {
        console.error(error);
        this.service.isLoading =false;
      });

  }

}
