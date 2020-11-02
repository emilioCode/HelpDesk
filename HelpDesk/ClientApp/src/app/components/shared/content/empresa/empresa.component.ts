import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../../../services/api.service';

@Component({
  selector: 'app-empresa',
  templateUrl: './empresa.component.html',
  styles: []
})
export class EmpresaComponent implements OnInit {

  constructor(private service: ApiService) { 
    this.getBusiness(this.service.getUser().id);
  }

  isLoading = false;
  empresas:any;
  option;
  empresa:any={};
  imageUrl;

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

  fillModal(option='add',object){
    this.option = option;
    this.empresa = object;
    console.log(this.empresa)
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
      this.empresa.image = this.imageUrl.split(',')[1];
      
    }
    reader.readAsDataURL(this.fileUpload);
  }

  add(){
    this.service.isLoading = true;
     this.service.http.post(this.service.baseUrl + 'api/Business',this.empresa,{headers:this.service.headers,responseType:'json'})
      .subscribe(res=>{
      console.log( res )
      this.service.swal(res.title,res.message,res.icon);
      if(res.code=="1") {
        this.getBusiness(this.service.getUser().id);
      }
      this.service.isLoading =false;
      },error => {
        console.error(error);
        this.service.isLoading =false;
      });
  }

  edit(){
    this.service.isLoading = true;
    this.empresa.habilitado = true;
     this.service.http.put(this.service.baseUrl + 'api/Business/'+this.empresa.id,this.empresa,{headers:this.service.headers,responseType:'json'})
      .subscribe(res=>{
      this.service.swal(res.title,res.message,res.icon);
      if(res.code=="1") {
        this.getBusiness(this.service.getUser().id);
      }
      this.service.isLoading =false;
      },error => {
        console.error(error);
        this.service.isLoading =false;
      });
  }
  delete(){
    this.service.isLoading = true;
    this.empresa.habilitado = !this.empresa.habilitado;
     this.service.http.put(this.service.baseUrl + 'api/Business/'+this.empresa.id,this.empresa,{headers:this.service.headers,responseType:'json'})
      .subscribe(res=>{
      this.service.swal(res.title,res.message,res.icon);
      if(res.code=="1") {
        this.getBusiness(this.service.getUser().id);
      }
      this.service.isLoading =false;
      },error => {
        console.error(error);
        this.service.isLoading =false;
      });

  }

}
