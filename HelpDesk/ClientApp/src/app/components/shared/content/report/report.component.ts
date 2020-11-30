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
    }
   }

  ngOnInit() {
  }

}
