import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { User } from 'src/models/user';
import { Guid } from 'guid-typescript';
import { Pricelist } from 'src/models/pricelist';
import { BusStop } from 'src/models/bus-stop';

@Injectable({
  providedIn: 'root'
})
export class BusStopService {
  api_route: String = 'http://localhost:52295/api/BusStops';

  constructor(private http: HttpClient) { }

  public getBusStop(busStopId: Guid): Observable<any>{
    return this.http.get(`${this.api_route}/${busStopId}`, {observe: 'response'});
  }

  public editBusStop(busStop: BusStop, eTag: string): Observable<any>{
    return this.http.put(`${this.api_route}/${busStop.Id}`, `Id=${busStop.Id}&X=${busStop.X}&Y=${busStop.Y}&Name=${busStop.Name}&Address=${busStop.Address}`,  { "headers" : {'etag': `${eTag}`, 'Content-type' : 'application/x-www-form-urlencoded'} } );
  }

  public addBusStop(busStop: BusStop): Observable<any>{
    return this.http.post(`${this.api_route}`, `Id=${busStop.Id}&X=${busStop.X}&Y=${busStop.Y}&Name=${busStop.Name}&Address=${busStop.Address}`,  { "headers" : {'Content-type' : 'application/x-www-form-urlencoded'}} );
  }

  public deleteBusStop(busStopId: Guid) :Observable<any>{
    return this.http.delete(`${this.api_route}/${busStopId}`);
  }
}