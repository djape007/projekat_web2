import { Component, OnInit, Inject, forwardRef } from '@angular/core';
import { HomeComponent } from '../home/home.component';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { BusStop } from 'src/models/bus-stop';
import { BusStopService } from '../services/bus-stop.service';
import { MatTableDataSource } from '@angular/material';
import { Guid } from 'guid-typescript';
import { BusStopsOnLine } from 'src/models/bus-stops-on-line';
import { BusStopsOnLineService } from '../services/bus-stops-on-line.service';
import { TimetableService } from '../services/timetable.service';

@Component({
  selector: 'app-edit-bus-stop',
  templateUrl: './edit-bus-stop.component.html',
  styleUrls: ['./edit-bus-stop.component.css']
})
export class EditBusStopComponent implements OnInit {

  //getBusStopForm: FormGroup;
  busStopForm: FormGroup;
  //lineForm: FormGroup;
  get selectedBusStop() : BusStop {
    return this._parent.editSelectedBusStop;
  }

  dataSource = new MatTableDataSource();
  displayedColumns: string[] = ['Id', 'Direction'];

  selectedLineId: string = "";

  msgOk: string = null;
  msgBad: string = null;

  eTag: string;

  constructor(@Inject(forwardRef(() => HomeComponent)) private _parent: HomeComponent, private formBuilder: FormBuilder,
    private _busStopService: BusStopService, private _timeTableService: TimetableService, private _busStopOnLinesService: BusStopsOnLineService ) { }

  ngOnInit() {
    this.getAllTimetables();
    this._parent.prikaziDesniMeni();
    this._parent.RemoveAllLinesFromMap();

    this.busStopForm = this.formBuilder.group({
      x: ['', Validators.nullValidator],
      y: ['', Validators.nullValidator],
      busStopName: ['', Validators.required],
      busStopLines: ['', Validators.required]
    });

    if (this.selectedBusStop !== null) {
      this._parent.removeOverlay();
    }
  }

  ngOnDestroy() {
    this._parent.RemoveAllLinesFromMap();
    Object.keys(this._parent.stanicePrikazaneNaMapi).forEach(idStanice => {
      this._parent.stanicePrikazaneNaMapi[idStanice].setMap(null);
    });
    this._parent.DeselectBusStop();
  }

  getAllTimetables(){
    this._timeTableService.getAllTimetables()
    .subscribe(
      data => {
        this.dataSource = new MatTableDataSource(this.createDataSource(data));
      },
      err => {
        console.log(err);
      }
    )
  }

  displayLineOnMap(lineId: any) {
    this._parent.DisplayLineOnMap(lineId, true, false);
    this._parent.DeselectBusStop();
  }

  //get getBusStopf() { return this.getBusStopForm.controls; }
  //get linef() { return this.lineForm.controls; }
  get busStopf() { return this.busStopForm.controls; }


  addBtnClick(){
    let tmpBusStop = new BusStop();
    tmpBusStop.Id = Guid.create();
    tmpBusStop.X = this._parent.map.getCenter().lat();
    tmpBusStop.Y = this._parent.map.getCenter().lng();
    this.busStopf.x.setValue(tmpBusStop.X);
    this.busStopf.y.setValue(tmpBusStop.Y);
    tmpBusStop.Name = "Nova stanica";
    
    if (this.selectedLineId.length > 0) {
      tmpBusStop.Address = this.selectedLineId;
    }
    this._parent.prikazaneStaniceJedneLinije = new Array<any>();
    this._parent.DrawBusStopOnMap(tmpBusStop, "LINE_NOT_SET", true);
    this._parent.removeOverlay();
  }

  SaveBusStop() {
    if (!this.selectedBusStop || !this._parent.editSelectedMarker) {
      return;
    }

    let linije: string = this.selectedBusStop.Address;
    if (linije.length == 0) {
      return;
    }

    if (linije.endsWith(',')) {
      linije = linije.slice(0, linije.length - 1);
    }
    this.selectedBusStop.Address = linije;

    let linijeList = linije.split(',');

    this._parent.editSelectedBusStop.X = this._parent.editSelectedMarker.getPosition().lat();
    this._parent.editSelectedBusStop.Y = this._parent.editSelectedMarker.getPosition().lng();

    this._busStopService.addBusStop(this.selectedBusStop).subscribe(
      dataBusStop => {
        console.log(dataBusStop);
        this.msgOk = "Stanica je dodata";
        this.msgBad = null;
        
        linijeList.forEach(idLinije => {
          let tmpBusStopOnLine = new BusStopsOnLine();
          tmpBusStopOnLine.BusStopId = dataBusStop['Id'];
          tmpBusStopOnLine.Id = Guid.create();
          tmpBusStopOnLine.LineId = idLinije.toUpperCase();

          this._busStopOnLinesService.addBusStopOnLine(tmpBusStopOnLine).subscribe(
            data => {
              console.log(data);
            },
            error => {
              console.log(error);
            }
          )
          });
      },
      error => {
        if (error['statusText'] == 'Conflict') {
          this._busStopService.editBusStop(this.selectedBusStop, this.selectedBusStop.etag).subscribe(
            data => {
              this.msgOk = "Izmene su sačuvane";
              this.msgBad = null;
            },
            error => {
              this.msgBad = null;
              this.msgOk = null;

              if (error['statusText'] == 'Precondition Failed') {
                this.msgBad = "Neko je vec izmenio stanicu!";
              } else {
                console.log(error);
                this.msgBad = error['statusText'];
              }
            }
          )
        } else {
          console.log(error);
          this.msgOk = null;
          this.msgBad = error['statusText'];
        }
      }
    );

  }

  RemoveBusStop() {
    if (!this.selectedBusStop || !this._parent.editSelectedMarker) {
      return;
    }

    this._busStopService.deleteBusStop(this.selectedBusStop.Id).subscribe();

    this._parent.editSelectedMarker.setMap(null);
    this._parent.editSelectedMarker = null;
    this._parent.editSelectedBusStop = null;

  }

  applyFilter(filterValue: string) {
    this.dataSource.filter = filterValue.trim().toLowerCase();
  }

  createDataSource(data: any): any{
    var retVal = new Array();
    for(let item of data){
      var pushVal = {
        Id : item.LineId,
        Direction : item.Line.Direction,
        LineNumber : (Number)(item.LineId.replace('A','').replace('B','').replace('A','').replace('B',''))
      };

      retVal.push(pushVal);      
    }
    return retVal.sort((x,y) => x.LineNumber - y.LineNumber);
  }

  selectRow(row: any){
    if (row.Id == this.selectedLineId) {
      this.selectedLineId = "";
      this._parent.RemoveLineFromMap(row.Id);
      this._parent.displayOverlay();
      this._parent.DeselectBusStop();
    } else {
      this.selectedLineId = row.Id;
      this.displayLineOnMap(this.selectedLineId);
      this._parent.removeOverlay();
    }
  }
}

