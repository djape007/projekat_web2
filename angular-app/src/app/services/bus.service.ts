import { Injectable, EventEmitter } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Bus } from 'src/models/bus';

declare var $;

@Injectable({
  providedIn: 'root'
})

export class BusService {
  private proxy: any;  
  private proxyName: string = 'busPositions';  
  private connection: any;  
  public connectionExists: Boolean; 

  public notificationReceived: EventEmitter <string>;  

  constructor(private http: HttpClient) {
      this.notificationReceived = new EventEmitter<string>();
      this.connectionExists = false;  
      // create a hub connection  
      this.connection = $.hubConnection("http://localhost:52295/");
      // create new proxy with the given name 
      this.proxy = this.connection.createHubProxy(this.proxyName);  
  }

  // browser console will display whether the connection was successful    
  public startConnection(): Observable<Boolean> { 
      
    return Observable.create((observer) => {
        this.connection.start()
        .done((data: any) => {  
            console.log('Now connected ' + data.transport.name + ' (Buses Location WS), connection ID=' + data.id)
            this.connectionExists = true;

            observer.next(true);
            observer.complete();
        })
        .fail((error: any) => {  
            console.log('Could not connect (Buses Location WS) ' + error);
            this.connectionExists = false;

            observer.next(false);
            observer.complete(); 
        });  
      });
  }

  public registerBusesLocations() : Observable<Array<Bus>> {
    return Observable.create((observer) => {
        this.proxy.on('newPositions', (data: string) => {
            let busevi = new Array<Bus>();
            //data je string koji je server prosledio sa python servisa za simuliranje
            //NS123TR,45.324234,19.234324,7A|NS123RR,19.34234,45.234234,22A
            data.split('|').forEach(busData => {
              let splitedData = busData.split(",");
              let bus = new Bus();
              bus.Id = splitedData[0];
              bus.X = Number.parseFloat(splitedData[1]);
              bus.Y = Number.parseFloat(splitedData[2]);
              bus.LineId = splitedData[3];
              busevi.push(bus);
            });
            
            observer.next(busevi);
        });  
    });
  }
}