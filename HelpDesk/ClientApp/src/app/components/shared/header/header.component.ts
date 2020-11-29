import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../../services/api.service';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html'
})
export class HeaderComponent implements OnInit {

  constructor(private service: ApiService) { }
  seeNotifications =false;
  ngOnInit() {
  }

  signOut(){
    // window.location.reload();
    //this.router.navigateByUrl('/login');
    this.service.swal({
      title: 'Cerrar Sesión?',
      text: "No se guardarán cambios que no hn sido guardados previamente.",
      type: 'info',
      showCancelButton: true,
      confirmButtonColor: '#3085d6',
      cancelButtonColor: '#d33'//,
      //confirmButtonText: 'cerrar'
    }).then((result) => {
      if (result.value) {
        this.service.swal({
          title: 'Cerrando Sesión',
          html: '<i class="fa fa-spinner fa-pulse fa-3x fa-fw"></i> <span class="sr-only">Cargando...</span>',
          //type: 'info',
          timer: 2000,
          showCancelButton: false,
          showConfirmButton: false
          //confirmButtonText: 'cerrar'
        }).then((result)=>{
          this.service.closeSession();
        });

        
        // window.location.reload();
      }
    })
  }
}
