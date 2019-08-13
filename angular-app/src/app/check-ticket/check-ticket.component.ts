import { Component, OnInit, forwardRef, Inject, ViewChild } from '@angular/core';
import { HomeComponent } from '../home/home.component';
import { TicketService } from '../services/ticket.service';
import { SoldTicket } from 'src/models/sold-ticket';

@Component({
  selector: 'app-check-ticket',
  templateUrl: './check-ticket.component.html',
  styleUrls: ['./check-ticket.component.css']
})
export class CheckTicketComponent implements OnInit {

  constructor(@Inject(forwardRef(() => HomeComponent)) private _parent: HomeComponent, private _ticketService: TicketService) { }
  validSoldTickets: Array<SoldTicket>;

  ticketId: string = "";
  foundTicket: any;
  message: string = "";
  isError:boolean = false;
  displayTicketData:boolean = true;

  ngOnInit() {
    this._parent.prikaziDesniMeni();
  }

  checkTicket() {
    if (this.ticketId.trim().length < 35) {
      this.DisplayMessage("ID karte nije ispravan",true);
      return;
    }

    this._ticketService.isTicketValid(this.ticketId).subscribe(
      data => {
        if (data == "valid") {
          this.DisplayMessage("Karta je validna",false);
        } else if (data == "expired") {
          this.DisplayMessage("Karta je istekla", true);
        }

        if (this.displayTicketData) {
          this._ticketService.getTicket(this.ticketId).subscribe(
            data2 => {
              this.foundTicket = data2;
            },
            error2 => {
              this.DisplayMessage(error2.error.Message, true);
            }
          );
        } else {
          this.foundTicket = null;
        }
      },
      error => {
        this.DisplayMessage(error.error.Message, true);
      }
    )
  }

  DisplayMessage(msg:string, isError:boolean) {
    if (msg == null) {
      this.message = "poruka je null :(";
      this.isError = true;
    } else {
      this.message = msg;
      this.isError = isError;
    }
  }

}
