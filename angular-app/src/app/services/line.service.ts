import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { Guid } from 'guid-typescript';
import { Line } from 'src/models/line';
import { sha256 } from 'js-sha256';

@Injectable({
  providedIn: 'root'
})
export class LineService {
  api_route: String = 'http://localhost:52295/api/Lines';

  constructor(private http: HttpClient) { }

  public getAllLines(): Observable<any> {
    return this.http.get(`${this.api_route}`, {observe: 'response'});
  }

  public getLine(id: string): Observable<any>{
    return this.http.get(`${this.api_route}/${id}`, {observe: 'response'}).pipe(
      tap(data => {
        data['body']['BusStopsOnLines'].forEach(busStopOnLine => {
          let busStop = busStopOnLine['BusStop'];
          let rawValue = `${busStop.Id}${busStop.Name}${busStop.X.toFixed(5)}${busStop.Y.toFixed(5)}${busStop.BusStopsOnLines.length + 1}`;
          //busStopOnLine['BusStop']['raw'] = rawValue;
          busStopOnLine['BusStop']['etag'] = sha256(rawValue);
        });
        let line = data['body'];
        let rawValue = `${line.Id}${line.Direction}${line.PointLinePaths.length}`;
        data['body']['etag'] = sha256(rawValue);
      }),
    );
  }

  public editLine(line: Line, eTag: string): Observable<any>{
    if ("etag" in line) {
      delete line["etag"];
    }
    let jsonObj = JSON.stringify(line);
    return this.http.put(`${this.api_route}/${line.Id}`, jsonObj ,{ "headers" : {'etag': `${eTag}`, 'Content-type' : 'application/json'}});
  }

  public addLine(line: Line): Observable<any>{
    if ("etag" in line) {
      delete line["etag"];
    }
    let jsonObj = JSON.stringify(line);
    return this.http.post(`${this.api_route}`, jsonObj ,{ "headers" : {'Content-type' : 'application/json'}});
   }

  public deleteLine(lineId: string) :Observable<any>{
    return this.http.delete(`${this.api_route}/${lineId}`);
  }
}