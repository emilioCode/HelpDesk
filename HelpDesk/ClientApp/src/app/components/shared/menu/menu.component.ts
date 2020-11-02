import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../../services/api.service';

@Component({
  selector: 'app-menu',
  templateUrl: './menu.component.html'
})
export class MenuComponent implements OnInit {

  constructor(private service: ApiService){}

  ngOnInit() {
  }
  

  adminLteConf = [
    {label: 'Configuracion',iconClasses: 'fa fa-cogs', children:[
      {label: 'Entidad', route: 'business',iconClasses:'fa fa-building', see: this.service.getLevel(this.service.getUser().acceso) >2},
      {label:'Usuarios', route:'user', iconClasses: 'fa fa-users', see: this.service.getLevel(this.service.getUser().acceso)>2},
      // {label: 'Servicios', route: 'service',iconClasses:'fa fa-tag'},
      // {label: 'Comprobantes', route: 'vouchers',iconClasses:'fa fa-ticket'}
        
    ]}
  ]
}
