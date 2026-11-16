import { Component, OnInit } from '@angular/core';
import { ApiService } from '../../../services/api.service';

@Component({
  selector: 'app-content',
  templateUrl: './content.component.html'
})
export class ContentComponent implements OnInit {

  constructor(public service: ApiService) { }

  ngOnInit() {
  }



}
