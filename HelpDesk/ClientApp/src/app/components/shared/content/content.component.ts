import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { ApiService } from '../../../services/api.service';

@Component({
  standalone: false,
  selector: 'app-content',
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './content.component.html'
})
export class ContentComponent implements OnInit {

  constructor(public service: ApiService) { }

  ngOnInit() {
  }



}
