import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { ApiService } from '../../../services/api.service';

@Component({
    selector: 'app-footer',
    changeDetection: ChangeDetectionStrategy.Eager,
    templateUrl: './footer.component.html'
})
export class FooterComponent implements OnInit {

  constructor(public service: ApiService){}

  ngOnInit() {
  }

}
