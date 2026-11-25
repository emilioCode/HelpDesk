import { Component, ChangeDetectionStrategy, HostListener } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../../services/api.service';
import { HdIconComponent } from '../../../ui/hd-icon.component';
import { ShellState } from '../../../ui/shell-state.service';
import { ThemeService } from '../../../ui/theme.service';

@Component({
  selector: 'app-header',
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.css'],
  imports: [RouterLink, HdIconComponent]
})
export class HeaderComponent {
  menuOpen = false;

  constructor(public service: ApiService, public shell: ShellState, public theme: ThemeService) { }

  get initials(): string {
    const name: string = this.service.getUser().nombre || '';
    return name.split(/\s+/).filter(x => x).slice(0, 2).map(x => x[0].toUpperCase()).join('');
  }

  get role(): string {
    const role: string = this.service.getUser().acceso || '';
    return role.charAt(0).toUpperCase() + role.slice(1).toLowerCase();
  }

  @HostListener('document:click')
  closeMenu() { this.menuOpen = false; }

  @HostListener('document:keydown.escape')
  closeOnEscape() { this.menuOpen = false; }

  toggleMenu(event: Event) {
    event.stopPropagation();
    this.menuOpen = !this.menuOpen;
  }

  signOut() {
    this.menuOpen = false;
    this.service.swal({
      title: 'Cerrar Sesión?',
      text: "No se guardarán cambios que no hn sido guardados previamente.",
      type: 'info',
      showCancelButton: true,
      confirmButtonColor: '#3085d6',
      cancelButtonColor: '#d33'
    }).then((result) => {
      if (result.value) {
        this.service.swal({
          title: 'Cerrando Sesión',
          html: '<i class="fa fa-spinner fa-pulse fa-3x fa-fw"></i> <span class="sr-only">Cargando...</span>',
          timer: 2000,
          showCancelButton: false,
          showConfirmButton: false
        }).then((result) => {
          this.service.closeSession();
        });
      }
    })
  }
}
