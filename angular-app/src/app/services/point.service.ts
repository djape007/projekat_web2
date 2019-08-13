import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Guid } from 'guid-typescript';
import { Line } from 'src/models/line';
import { PointPathLine } from 'src/models/point-path-line';

@Injectable({
  providedIn: 'root'
})
export class PointService {
  api_route: String = 'http://localhost:52295/api/PointPathLines';

  constructor(private http: HttpClient) { }

  public addPoint(point: PointPathLine): Observable<any>{
    return this.http.post(`${this.api_route}`, `Id=${point.Id}&X=${point.X}&Y=${point.Y}&LineId=${point.LineId}&SequenceNumber=${point.SequenceNumber}`,  { "headers" : {'Content-type' : 'application/x-www-form-urlencoded'}} );
  }

  public deletePoint(point: PointPathLine): Observable<any>{
    return this.http.delete(`${this.api_route}/${point.Id}`);
  }
}