import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { User } from 'src/models/user';
import { Guid } from 'guid-typescript';
import { PriceHistory } from 'src/models/price-history';

@Injectable({
  providedIn: 'root'
})
export class PricehistoryService {
  api_route: String = 'http://localhost:52295/api/PriceHistories';

  constructor(private http: HttpClient) { }

  public getSelectedPricehistories(pricelistId: Guid): Observable<any>{
    return this.http.get(`${this.api_route}/Pricelist/${pricelistId}`,{ "headers" : {'Content-type' : 'application/x-www-form-urlencoded'}} );
  }

  public addPricehistory(pricehistory: PriceHistory): Observable<any>{
    return this.http.post(`${this.api_route}`,`Id=${pricehistory.Id}&PricelistId=${pricehistory.PricelistId}&ProductTypeId=${pricehistory.ProductTypeId}&Price=${pricehistory.Price}`, { "headers" : {'Content-type' : 'application/x-www-form-urlencoded'}})
  }

  public editPricehistory(pricehistory: PriceHistory): Observable<any>{
    return this.http.put(`${this.api_route}/${pricehistory.Id}`,`Id=${pricehistory.Id}&PricelistId=${pricehistory.PricelistId}&ProductTypeId=${pricehistory.ProductTypeId}&Price=${pricehistory.Price}`, { "headers" : {'Content-type' : 'application/x-www-form-urlencoded'}})
  }
}