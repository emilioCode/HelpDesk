import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { Waiting2Component } from './waiting2.component';

describe('Waiting2Component', () => {
  let component: Waiting2Component;
  let fixture: ComponentFixture<Waiting2Component>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ Waiting2Component ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(Waiting2Component);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
