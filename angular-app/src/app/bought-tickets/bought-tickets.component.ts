import { Component, OnInit, Inject, forwardRef, ViewChild } from '@angular/core';
import { HomeComponent } from '../home/home.component';
import { SoldTicket } from 'src/models/sold-ticket';
import { MatTableDataSource, MatSort } from '@angular/material';
import { TicketService } from '../services/ticket.service';

@Component({
  selector: 'app-bought-tickets',
  templateUrl: './bought-tickets.component.html',
  styleUrls: ['./bought-tickets.component.css']
})
export class BoughtTicketsComponent implements OnInit {

  constructor(@Inject(forwardRef(() => HomeComponent)) private _parent: HomeComponent, private _ticketService: TicketService) { }

  currentDateTime: any = Date.now();
  selectedTicket: SoldTicket = null;
  kupljeneKarte: MatTableDataSource<SoldTicket> = new MatTableDataSource<SoldTicket>();
  displayedColumns: any = ['Type', 'DateOfPurchase', 'Expires'];
  @ViewChild(MatSort) sort: MatSort;

  ngOnInit() {
    this._parent.prikaziDesniMeni();
    this.getUserTickets();
  }

  getUserTickets() {
    this._ticketService.getUserTickets().subscribe(
      data => {
        this.updateCurrentDateTime();
        //console.log(data);
        data.forEach(element => {
          element.Expires = Date.parse(element.Expires);
          element.DateOfPurchase = Date.parse(element.DateOfPurchase);
        });
        this.kupljeneKarte = new MatTableDataSource(data.sort((x,y) => x.DateOfPurchase < y.DateOfPurchase));
        this.kupljeneKarte.sort = this.sort;
      },
      error => {
        console.log(error);
      }
    );
  }

  updateCurrentDateTime() {
    this.currentDateTime = Date.now();
  }

  applyFilter(search:string) {
    //sacuvam original funkciju filtriranja
    let originalFilterPredicate = this.kupljeneKarte.filterPredicate;

    if (search.toLowerCase().includes("validn")) {
      this.updateCurrentDateTime();
      this.kupljeneKarte.filterPredicate = (data: SoldTicket, filter: string) => {
        return data.Expires > this.currentDateTime;
      };
    }

    this.kupljeneKarte.filter = search.toLowerCase();
    //ovde vratim original
    this.kupljeneKarte.filterPredicate = originalFilterPredicate;
  }

  selectTicket(karta: SoldTicket) {
    this.selectedTicket = karta;
  }

}
