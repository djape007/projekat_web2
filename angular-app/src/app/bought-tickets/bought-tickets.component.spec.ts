import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { BoughtTicketsComponent } from './bought-tickets.component';

describe('BoughtTicketsComponent', () => {
  let component: BoughtTicketsComponent;
  let fixture: ComponentFixture<BoughtTicketsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ BoughtTicketsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(BoughtTicketsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
