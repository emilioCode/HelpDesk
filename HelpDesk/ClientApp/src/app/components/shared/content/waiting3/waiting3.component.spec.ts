import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { Waiting3Component } from './waiting3.component';

describe('Waiting3Component', () => {
  let component: Waiting3Component;
  let fixture: ComponentFixture<Waiting3Component>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ Waiting3Component ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(Waiting3Component);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
