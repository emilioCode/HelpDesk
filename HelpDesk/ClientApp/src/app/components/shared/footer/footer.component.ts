import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { HdDialogComponent } from '../../../ui/hd-dialog.component';
import { ApiService } from '../../../services/api.service';

@Component({
    selector: 'app-footer',
    changeDetection: ChangeDetectionStrategy.Eager,
    imports: [HdDialogComponent],
    templateUrl: './footer.component.html'
})
export class FooterComponent implements OnInit {

  constructor(public service: ApiService){}

  ngOnInit() {
  }

}
