import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
    selector: 'app-setting',
    changeDetection: ChangeDetectionStrategy.Eager,
    templateUrl: './setting.component.html',
    imports: [FormsModule]
})
export class SettingComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
