import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Guid } from 'guid-typescript';

@Injectable({
  providedIn: 'root'
})
export class TicketService {
  api_route: String = 'http://localhost:52295/api';

  constructor(private http: HttpClient) { }

  public getAllPriceHistories(): Observable<any>{
    return this.http.get(`${this.api_route}/PriceHistories`);
  }

  public getCurrentPriceHistories(): Observable<any>{
    return this.http.get(`${this.api_route}/PriceHistories/Current`);
  }

  public getAllCoefficients(): Observable<any>{
    return this.http.get(`${this.api_route}/Coefficients`);
  }

  public buyTicket(productTypeId: Guid): Observable<any>{
    return this.http.post(`${this.api_route}/SoldTickets/Buy/${productTypeId}`,'');
  }

  public buyTicketAnonymous(): Observable<any>{
    return this.http.post(`${this.api_route}/SoldTickets/BuyAnonymous`,'');
  }

  public isTicketValid(id: string): Observable<any> {
    return this.http.get(`${this.api_route}/SoldTickets/Valid/${id}`);
  }

  public getTicket(id: string): Observable<any> {
    return this.http.get(`${this.api_route}/SoldTickets/${id}`);
  }

  public getUserTickets(): Observable<any> {
    return this.http.get(`${this.api_route}/SoldTickets/GetUserTickets`);
  }
}