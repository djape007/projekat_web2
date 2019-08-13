import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ProductTypeService {
  api_route: String = 'http://localhost:52295/api/ProductTypes';

  constructor(private http: HttpClient) { }

  public getAllProductTypes(): Observable<any>{
    return this.http.get(`${this.api_route}`);
  }
}