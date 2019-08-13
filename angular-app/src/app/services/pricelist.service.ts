import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { User } from 'src/models/user';
import { Guid } from 'guid-typescript';
import { Pricelist } from 'src/models/pricelist';

@Injectable({
  providedIn: 'root'
})
export class PricelistService {
  api_route: String = 'http://localhost:52295/api/Pricelists';

  constructor(private http: HttpClient) { }

  public getAllPricelists(): Observable<any>{
    return this.http.get(`${this.api_route}`);
  }

  public addPricelit(id: Guid, from: string, to: string): Observable<any>{
    return this.http.post(`${this.api_route}`,`Id=${id}&From=${from}&To=${to}`, { "headers" : {'Content-type' : 'application/x-www-form-urlencoded'}})
  } 

  public editPricelit(id: Guid, from: string, to: string, etag: string): Observable<any>{
    return this.http.put(`${this.api_route}/${id}`,`Id=${id}&From=${from}&To=${to}`, { "headers" : {'etag': `${etag}`,'Content-type' : 'application/x-www-form-urlencoded'}})
  } 

  public deletePricelist(id: Guid): Observable<any>{
    return this.http.delete(`${this.api_route}/${id}`, { "headers" : {'Content-type' : 'application/x-www-form-urlencoded'}})
  } 
}