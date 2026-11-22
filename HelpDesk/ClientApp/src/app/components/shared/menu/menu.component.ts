import { Component, ChangeDetectionStrategy } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { ApiService } from '../../../services/api.service';
import { HdIconComponent } from '../../../ui/hd-icon.component';
import { ShellState } from '../../../ui/shell-state.service';

interface NavItem { label: string; route: string; icon: string; see: boolean; }
interface NavGroup { title: string; items: NavItem[]; }

@Component({
  selector: 'app-menu',
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './menu.component.html',
  styleUrls: ['./menu.component.css'],
  imports: [RouterLink, RouterLinkActive, HdIconComponent]
})
export class MenuComponent {
  groups: NavGroup[];

  constructor(public service: ApiService, public shell: ShellState) {
    // What each role sees is the same as before: the level of the role against the level each entry asks for.
    const level = this.service.getLevel(this.service.getUser().acceso);
    const groups: NavGroup[] = [
      { title: 'Trabajo', items: [
        { label: 'Inicio', route: '/', icon: 'layout-dashboard', see: true },
        { label: 'Órdenes de servicio', route: '/ticket', icon: 'ticket', see: level > 1 },
        { label: 'Buzón', route: '/inbox', icon: 'inbox', see: level > 0 },
        { label: 'Sin asignar', route: '/waiting', icon: 'hourglass', see: level > 1 },
        { label: 'Completadas', route: '/finished', icon: 'circle-check', see: level > 1 },
        { label: 'Calendario', route: '/calendar', icon: 'calendar', see: true }
      ] },
      { title: 'Gestión', items: [
        { label: 'Clientes', route: '/costumer', icon: 'users', see: level > 0 },
        { label: 'Informes', route: '/report', icon: 'chart-column', see: level > 2 }
      ] },
      { title: 'Configuración', items: [
        { label: 'Empresa', route: '/business', icon: 'building-2', see: level > 2 },
        { label: 'Usuarios', route: '/user', icon: 'users', see: level > 2 },
        { label: 'Mi perfil', route: '/profile', icon: 'user', see: true }
      ] }
    ];
    this.groups = groups.map(g => ({ title: g.title, items: g.items.filter(i => i.see) })).filter(g => g.items.length > 0);
  }
}
