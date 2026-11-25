import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { ApiService } from '../../../services/api.service';
import { RouterOutlet } from '@angular/router';

@Component({
    selector: 'app-content',
    changeDetection: ChangeDetectionStrategy.Eager,
    templateUrl: './content.component.html',
    imports: [RouterOutlet]
})
export class ContentComponent implements OnInit {

  constructor(public service: ApiService) { }

  ngOnInit() {
  }



}
