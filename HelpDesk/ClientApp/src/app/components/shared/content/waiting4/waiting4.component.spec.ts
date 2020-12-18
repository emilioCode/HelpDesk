import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { Waiting4Component } from './waiting4.component';

describe('Waiting4Component', () => {
  let component: Waiting4Component;
  let fixture: ComponentFixture<Waiting4Component>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ Waiting4Component ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(Waiting4Component);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
