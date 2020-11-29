import { Component } from '@angular/core';
import { ApiService } from '../services/api.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
})
export class HomeComponent {

  constructor(private service: ApiService){
    this.numbersOfTickets(this.service.getUser().id)
  }
  numbers:any = {};
  numbersOfTickets(idUser){
    this.service.isLoading = true;
    this.service.http.get(this.service.baseUrl + 'api/Ticket/numbersOfTickets/'+ idUser ,{headers:this.service.headers,responseType:'json'})
      .subscribe(res=>{
        this.numbers =  res.data;
        console.log( this.numbers );
        this.service.isLoading = false;
      },error => {
        console.error(error);
        this.service.isLoading = false;
      });
  }
}
