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
      {label:'Usuarios', route:'user', iconClasses: 'fa fa-street-view', see: this.service.getLevel(this.service.getUser().acceso)>2},
      // {label: 'Comprobantes', route: 'vouchers',iconClasses:'fa fa-ticket'}  
    ]},
    {label: 'Marco de trabajo',iconClasses: 'fa fa-level-down', children:[
      {label:'Clientes', route:'costumer', iconClasses: 'fa fa-users', see: this.service.getLevel(this.service.getUser().acceso)>0},
      {label:'Tickets', route:'ticket', iconClasses: 'fa fa-ticket', see: this.service.getLevel(this.service.getUser().acceso)>1},
      {label:'Buzón', route:'inbox', iconClasses: 'fa fa-inbox', see: this.service.getLevel(this.service.getUser().acceso)>0}
    ]}
  ]
}

