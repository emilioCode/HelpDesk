import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { ApiService } from '../../../services/api.service';
import { NgIf, NgFor } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { FilterPipe } from '../../../pipes/filter.pipe';

@Component({
    selector: 'app-menu',
    changeDetection: ChangeDetectionStrategy.Eager,
    templateUrl: './menu.component.html',
    imports: [NgIf, FormsModule, RouterLink, NgFor, FilterPipe]
})
export class MenuComponent implements OnInit {
  menuSearch: any = "";

  constructor(public service: ApiService){}

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
      {label:'Ordenes de servicio', route:'ticket', iconClasses: 'fa fa-ticket', see: this.service.getLevel(this.service.getUser().acceso)>1},
      {label:'Buzón', route:'inbox', iconClasses: 'fa fa-inbox', see: this.service.getLevel(this.service.getUser().acceso)>0},
      {label:'Reportes', route:'report', iconClasses: 'fa fa-pie-chart', see: this.service.getLevel(this.service.getUser().acceso)>2},
      {label: 'Calendario', route:'calendar', iconClasses:'fa fa-calendar', see:true}
    ]}
  ]
}

