import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { EditPricelistComponent } from './edit-pricelist.component';

describe('EditPricelistComponent', () => {
  let component: EditPricelistComponent;
  let fixture: ComponentFixture<EditPricelistComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ EditPricelistComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(EditPricelistComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
