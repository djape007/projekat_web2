import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Guid } from 'guid-typescript';
import { Line } from 'src/models/line';
import { PointPathLine } from 'src/models/point-path-line';
import { BusStopsOnLine } from 'src/models/bus-stops-on-line';

@Injectable({
  providedIn: 'root'
})
export class BusStopsOnLineService {
  api_route: String = 'http://localhost:52295/api/BusStopsOnLines';

  constructor(private http: HttpClient) { }

  public addBusStopOnLine(busStopOnLine: BusStopsOnLine): Observable<any>{
    return this.http.post(`${this.api_route}`, `Id=${busStopOnLine.Id}&LineId=${busStopOnLine.LineId}&BusStopId=${busStopOnLine.BusStopId}`,  { "headers" : {'Content-type' : 'application/x-www-form-urlencoded'}} );
  }

  public deleteBusStopOnLine(busStopOnLine: BusStopsOnLine): Observable<any>{
    return this.http.delete(`${this.api_route}/${busStopOnLine.Id}`);
  }
}