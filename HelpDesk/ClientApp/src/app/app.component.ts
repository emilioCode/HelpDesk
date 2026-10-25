import { Component, OnInit } from '@angular/core';
import { ApiService } from './services/api.service';
import { SessionService } from './services/session.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  constructor(private service: ApiService, private session: SessionService){ }

  ngOnInit(){
    this.session.start();
  }
  
  title = this.service.aplicationName;
  

}
