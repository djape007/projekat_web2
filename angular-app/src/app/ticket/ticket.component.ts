import { Component, OnInit, Inject, forwardRef, ViewChild } from '@angular/core';
import { HomeComponent } from '../home/home.component';
import { FormBuilder } from '@angular/forms';
import { TicketService } from '../services/ticket.service';
import { PriceHistory } from 'src/models/price-history';
import { Coefficient } from 'src/models/coefficient';
import { PricelistElement } from 'src/models/pricelist-element';
import { MatTableDataSource, MatSort } from '@angular/material';
import { Guid } from 'guid-typescript';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-ticket',
  templateUrl: './ticket.component.html',
  styleUrls: ['./ticket.component.css']
})
export class TicketComponent implements OnInit {

  priceHistories: Array<PriceHistory> = new Array<PriceHistory>();
  coefficients: Array<Coefficient> = new Array<Coefficient>();
  selectedRowIndex: number;
  selectedRowElement: PricelistElement;
  message: string = "";
  messageIsError: boolean = false;

  lastBoughtTicketID: string = "";
  lastBoughtTicketValidUntil: number = -1;
  lastBoughtTicketPrice: Number = -1;

  displayedColumns: string[] = ['productTypeName', 'person','price'];
  dataSource = new MatTableDataSource();
  @ViewChild(MatSort) sort: MatSort;

  constructor(@Inject(forwardRef(() => HomeComponent)) private _parent: HomeComponent,
    private formBuilder: FormBuilder, private _service: TicketService, private _auth: AuthService) { }

  ngOnInit() {
    this._parent.prikaziLeviMeni();

    this._service.getCurrentPriceHistories()
      .subscribe(
        data => {
          this.priceHistories = data;
          this._service.getAllCoefficients()
            .subscribe(
              data => {
                this.coefficients = data;
                let priceList = this.createPricelist();//.sort((a,b) => Number(b.purchasable) - Number(a.purchasable))
                this.dataSource = new MatTableDataSource(priceList.sort((a,b) => Number(b.purchasable) - Number(a.purchasable)));
                this.dataSource.sort = this.sort;
              },
              err => {
                console.log(err);
              }
            );
        },
        err => {
          console.log(err);
        }
      );

  }

  UserCanBuy(row: PricelistElement) {
    if(this._auth.getToken() === null){
      if (row.person == "obican" && row.productTypeName == "Vremenska (1h)") {
        return true;
      } else {
        row.purchasable = false;
        return false;
      }
    } else {
      if (this._auth.getUserStatus().toLowerCase() == "verified") {
        if (row.person == this._auth.getUserType().toLowerCase()) {
          return true;
        } else {
          return false;
        }
      } else {
        if (row.person == "obican") {
          return true;
        } else {
          return false;
        }
      }
    }
  }

  applyFilter(filterValue: string) {
    this.dataSource.filter = filterValue.trim().toLowerCase();
  }

  DisplayMessage(text:string, error:boolean) {
    this.message = text;
    this.messageIsError = error;
  }

  createPricelist() : PricelistElement[]{
    var index = -1;
    var retVal = new Array<PricelistElement>();
    for(let priceHistory of this.priceHistories){
      for(let coef of this.coefficients){
        var pricelistEl = new PricelistElement();
        pricelistEl.productTypeName = priceHistory.ProductType.Name;
        pricelistEl.person = coef.Type;
        pricelistEl.price = priceHistory.Price * coef.Value;
        pricelistEl.id = ++index;
        pricelistEl.productTypeId = priceHistory.ProductType.Id;
        pricelistEl.purchasable = true;

        pricelistEl.purchasable = this.UserCanBuy(pricelistEl);

        retVal.push(pricelistEl);
      }
    }
    return retVal;
  }

  selectTicket(row: PricelistElement){
    if (row.purchasable) {
      this.selectedRowIndex = row.id;
      this.selectedRowElement = row;
    }
  }

  buyTicket(row: PricelistElement){
    this.DisplayMessage("", false);
    if(row==undefined){
      this.DisplayMessage("Izaberite kartu korisniče", true);
      return;
    }

    if(this._auth.getToken() != null){
      this._service.buyTicket(row.productTypeId)
        .subscribe(
          data => {
            this.lastBoughtTicketID = data.Id;
            this.lastBoughtTicketPrice = data.Price;
            this.lastBoughtTicketValidUntil = Date.parse(data.Expires);
            this.DisplayMessage("Karta je kupljena", false);
          },
          err => {
            this.DisplayMessage(err.error.Message, true);
          }
        )
    }else{
      this._service.buyTicketAnonymous()
        .subscribe(
          data => {
            this.lastBoughtTicketID = data.Id;
            this.lastBoughtTicketPrice = data.Price;
            this.lastBoughtTicketValidUntil = Date.parse(data.Expires);
            this.DisplayMessage("Karta je kupljena", false);
          },
          err => {
            this.DisplayMessage(err.error.Message, true);
          }
        )
    }

  }
}
