import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

@Component({
  standalone: false,
  selector: 'app-setting',
  changeDetection: ChangeDetectionStrategy.Eager,
  templateUrl: './setting.component.html'
})
export class SettingComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
