import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { CostumerComponent } from './costumer.component';

describe('CostumerComponent', () => {
  let component: CostumerComponent;
  let fixture: ComponentFixture<CostumerComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
    imports: [CostumerComponent]
})
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(CostumerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
