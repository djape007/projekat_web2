import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { EditBusStopComponent } from './edit-bus-stop.component';

describe('EditBusStopComponent', () => {
  let component: EditBusStopComponent;
  let fixture: ComponentFixture<EditBusStopComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ EditBusStopComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(EditBusStopComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
