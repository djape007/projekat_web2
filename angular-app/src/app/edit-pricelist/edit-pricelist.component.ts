import { Component, OnInit, Inject, forwardRef } from '@angular/core';
import { HomeComponent } from '../home/home.component';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatTableDataSource } from '@angular/material';
import { Guid } from 'guid-typescript';
import { PricelistService } from '../services/pricelist.service';
import { Pricelist } from 'src/models/pricelist';
import { PricehistoryService } from '../services/pricehistory.service';
import { PriceHistory } from 'src/models/price-history';
import { forEach } from '@angular/router/src/utils/collection';
import { ProductType } from 'src/models/product-type';
import { ProductTypeService } from '../services/product-type.service';
import { sha256 } from 'js-sha256';

@Component({
  selector: 'app-edit-pricelist',
  templateUrl: './edit-pricelist.component.html',
  styleUrls: ['./edit-pricelist.component.css']
})
export class EditPricelistComponent implements OnInit {

  priceForm: FormGroup;
  
  dataSource = new MatTableDataSource();
  displayedColumns: string[] = ['Index','From', 'To'];

  selectedRowIndex: Guid;

  pricelists : Array<Pricelist>;

  pricelist: Pricelist;
  newPricelist: Pricelist;
  pricehistories: Array<PriceHistory>;

  productTypes: Array<ProductType>;

  constructor(@Inject(forwardRef(() => HomeComponent)) private _parent: HomeComponent, private formBuilder: FormBuilder,
    private _pricelistService: PricelistService, private _pricehistoryService: PricehistoryService,
    private _productTypeService: ProductTypeService) { }

  ngOnInit() {
    this._parent.prikaziDesniMeni();

    this.priceForm = this.formBuilder.group({
      from: ['', Validators.required],
      to: ['', Validators.required],
      vremenska: ['', Validators.required],
      dnevna: ['', Validators.required],
      mesecna: ['', Validators.required],
      sestomesecna: ['', Validators.required],
      godisnja: ['', Validators.required]
    });

    this.getAllPricelists();

    this._productTypeService.getAllProductTypes()
      .subscribe(
        data => {
          this.productTypes = data;
        },
        err =>{
          console.log(err);
        }
      )

  }

  getAllPricelists(){
    this._pricelistService.getAllPricelists()
      .subscribe(
        data => {
          this.pricelists = data;
          this.dataSource = new MatTableDataSource(this.createDataSource(data));
          this.pricelists.forEach(element => {
            let prices: string = '';
            element.PriceHistories.forEach(element => {
              prices = prices.concat(element.Price.toString());
            });

            let from = new Date(element.From).toLocaleDateString().split('/');
            let fromString = `${from[2]}-${from[0]}-${from[1]}`;
            let to = new Date(element.To).toLocaleDateString().split('/');
            let toString = `${to[2]}-${to[0]}-${to[1]}`;

            let rawValue = `${element.Id}${fromString}${toString}${prices}`;
            element['etag'] = sha256(rawValue);
          });
        },
        err =>{
          console.log(err);
        }
      )
  }

  get f() { return this.priceForm.controls; }

  createDataSource(data: any): any{
    let retVal = new Array();
    let index = 0;
    for(let item of data){
      let pushVal = {
        Id : item.Id,
        From : this.createDate(item.From),
        To : this.createDate(item.To),
        Index: ++index
      };
      retVal.push(pushVal);      
    }
    return retVal;
  }

  createDate(date: any): string{
    let dateObj = new Date(date);
    let rval = dateObj.toLocaleDateString();
    return rval;
  }

  selectRow(row: any){
    this.selectedRowIndex = row.Id;
    this.pricelist = null;
    this.newPricelist = null;
  }

  editBtnClick(){
    this.pricelist = this.pricelists.find(x => x.Id == this.selectedRowIndex);
    this.newPricelist = null;
    this._pricehistoryService.getSelectedPricehistories(this.selectedRowIndex)
      .subscribe(
        data => {
          this.pricehistories = data;
          this.setFormData();
        },
        err => {
          console.log(err);
        }
      )
  }

  setFormData(){
    this.f.from.setValue(new Date(this.pricelist.From));
    this.f.to.setValue(new Date(this.pricelist.To));
    this.pricehistories.forEach(element => {
      let strName = element.ProductType.Name.split(' ')[0].toLowerCase();
      this.f[strName].setValue(element.Price);
    });
  }

  addBtnClick(){
    this.pricelist = null;
    this.newPricelist = new Pricelist();
    this.pricehistories = null;
    this.priceForm.reset();
    this.selectedRowIndex = null;
  }

  editPricelist(){
    if(this.priceForm.invalid)
      return;

    let from = this.f.from.value.toLocaleDateString().split('/');
    let fromString = `${from[2]}-${from[0]}-${from[1]}`;
    let to = this.f.to.value.toLocaleDateString().split('/');
    let toString = `${to[2]}-${to[0]}-${to[1]}`;

    this.pricehistories.forEach(element => {
      let strName = element.ProductType.Name.split(' ')[0].toLowerCase();
      element.Price = this.priceForm.controls[strName].value;
    });

    this._pricelistService.editPricelit(this.pricelist.Id, fromString, toString, this.pricelist['etag'])
    .subscribe(
      data => {
        this.pricehistories.forEach(element => {
          this._pricehistoryService.editPricehistory(element)
            .subscribe(
              data => {},
              err => {
                console.log(err);
              }
            )
        });
        this.pricelist = null;
        this.pricehistories = null;
        this.getAllPricelists();
      },
      err => {
        console.log(err);
      }
    )

  }

  addPricelist(){
    if(this.priceForm.invalid)
      return;
    
    let from = this.f.from.value.toLocaleDateString().split('/');
    let fromString = `${from[2]}-${from[0]}-${from[1]}`;
    let to = this.f.to.value.toLocaleDateString().split('/');
    let toString = `${to[2]}-${to[0]}-${to[1]}`;
    let id = Guid.create();

    let pricehostoriesArr = new Array<PriceHistory>();
    this.productTypes.forEach(element => {
      let pricehistory = new PriceHistory();
      pricehistory.Id = Guid.create();
      pricehistory.PricelistId = id;
      pricehistory.ProductTypeId = element.Id;
      let strName = element.Name.split(' ')[0].toLowerCase();
      pricehistory.Price = this.priceForm.controls[strName].value;
      pricehostoriesArr.push(pricehistory);
    });

    this._pricelistService.addPricelit(id, fromString, toString)
      .subscribe(
        data => {
          pricehostoriesArr.forEach(element => {
            this._pricehistoryService.addPricehistory(element)
              .subscribe(
                data => {},
                err => {
                  console.log(err);
                }
              )
          });
          this.newPricelist = null;
          this.getAllPricelists();
        },
        err => {
          console.log(err);
        }
      )
  }

  deleteBtnClick(){
    this._pricelistService.deletePricelist(this.selectedRowIndex)
      .subscribe(
        data => {
          this.getAllPricelists();
          this.selectedRowIndex = null;
          this.pricelist = null;
          this.newPricelist = null;
          this.priceForm.reset();
        },
        err => {
          console.log(err);
        }
      )
  }

  vremenskaIsNotNumber(){
    let val = this.f.vremenska.value;
    if(isNaN(val))
      this.f.vremenska.setErrors({'notnumber': true});
  }

  dnevnaIsNotNumber(){
    let val = this.f.dnevna.value;
    if(isNaN(val))
      this.f.dnevna.setErrors({'notnumber': true});
  }

  mesecnaIsNotNumber(){
    let val = this.f.mesecna.value;
    if(isNaN(val))
      this.f.mesecna.setErrors({'notnumber': true});
  }

  sestomesecnaIsNotNumber(){
    let val = this.f.sestomesecna.value;
    if(isNaN(val))
      this.f.sestomesecna.setErrors({'notnumber': true});
  }

  godisnjaIsNotNumber(){
    let val = this.f.godisnja.value;
    if(isNaN(val))
      this.f.godisnja.setErrors({'notnumber': true});
  }
}
