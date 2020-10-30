import { Component, OnInit } from '@angular/core';
import Swal from 'sweetalert2';
import { ApiService } from '../../../services/api.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html'
})
export class LoginComponent implements OnInit {

  constructor(private service: ApiService) { }

  ngOnInit() {
    // Swal('Campos requeridos','Ambos campos son necesarios','warning');
  }

}
