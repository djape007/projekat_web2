import { Component, OnInit, Inject, forwardRef } from '@angular/core';
import { HomeComponent } from '../home/home.component';
import { LineService } from '../services/line.service';
import { FormGroup, Validators, FormBuilder } from '@angular/forms';
import { Line } from 'src/models/line';
import { MatTableDataSource } from '@angular/material';
import { PointPathLine } from 'src/models/point-path-line';
import { Guid } from 'guid-typescript';
import { PointService } from '../services/point.service';
import { TimetableService } from '../services/timetable.service';
import { ThrowStmt } from '@angular/compiler';

@Component({
  selector: 'app-edit-line',
  templateUrl: './edit-line.component.html',
  styleUrls: ['./edit-line.component.css']
})
export class EditLineComponent implements OnInit {
  getLineForm: FormGroup;
  lineForm: FormGroup;
  errId: boolean = false;
  errDirection: boolean = false;

  dataSource = new MatTableDataSource();
  displayedColumns: string[] = ['Id', 'Direction'];

  selectedLineId: string = "";
  isNewLine: boolean = false;

  msgOk: string = null;
  msgBad: string = null;

  get selectedLine() {
    return this._parent.editSelectedLine;
  }

  eTag: string = '';

  constructor(@Inject(forwardRef(() => HomeComponent)) private _parent: HomeComponent, private _lineService: LineService) { }

  ngOnInit() {
    this._parent.prikaziDesniMeni();
    this.isNewLine = false;
    this.getAllLines();
  }

  ngOnDestroy() {
    this._parent.RemoveAllLinesFromMap();
  }

  getAllLines(){
    this._lineService.getAllLines()
    .subscribe(
      data => {
        this.dataSource = new MatTableDataSource(this.createDataSource(data['body']));
      },
      err => {
        console.log(err);
      }
    )
  }

  addNewLineBtnClick(){
    this.isNewLine = true;
    this.selectedLineId = "";
    let tmpLine = new Line();
    tmpLine.Id = "00A";
    tmpLine.Direction = "Not set";
    let start = new PointPathLine();
    start.SequenceNumber = 1;
    //start krece od zeleznicke stanice
    start.X = 45.2642883;
    start.Y = 19.8298207;
    let end = new PointPathLine();
    end.SequenceNumber = 2;
    end.X = this._parent.map.getCenter().lat();
    end.Y = this._parent.map.getCenter().lng();
    tmpLine.PointLinePaths.push(start);
    tmpLine.PointLinePaths.push(end);
    this._parent.RemoveAllLinesFromMap();
    this._parent.removeOverlay();
    this._parent.DrawLineOnMap(tmpLine, false, true);
  }

  createDataSource(data: any): any{
    var retVal = new Array();
    for(let item of data){
      var pushVal = {
        Id : item.Id,
        Direction : item.Direction,
        LineNumber : (Number)(item.Id.replace('A','').replace('B','').replace('A','').replace('B',''))
      };

      retVal.push(pushVal);      
    }
    return retVal.sort((x,y) => x.LineNumber - y.LineNumber);
  }

  applyFilter(filterValue: string) {
    this.dataSource.filter = filterValue.trim().toLowerCase();
  }

  saveLine(){
    this.errDirection = false;
    this.errId = false;
    //console.log(this.selectedLine);
    if (this.selectedLine.Id.trim().length == 0) {
      this.errId = true;
      return;
    }

    if (this.selectedLine.Direction.trim().length == 0) {
      this.errDirection = true;
      return;
    }

    let newPointPathLines = new Array<PointPathLine>();
    this._parent.editSelectedLinePath.getPath().forEach((pair, indx) => {
      let ppl = new PointPathLine();
      ppl.Id = Guid.create().toString();
      ppl.SequenceNumber = (indx + 1);
      ppl.X = pair.lat();
      ppl.Y = pair.lng();
      ppl.LineId = this.selectedLine.Id;
      newPointPathLines.push(ppl);
    });

    this.selectedLine.PointLinePaths = newPointPathLines;
    //this.selectedLine.Direction = 

    this.msgOk = null;
    this.msgBad = null;

    if (this.isNewLine) {
      this._lineService.addLine(this.selectedLine).subscribe(
        data => {
          //console.log(data);
          this.msgOk = "Nova linija je dodata";
          this.isNewLine = false;
          this.getAllLines();
        },
        error => {
          //console.log(error);
          this.msgBad = "Greška";
        }
      );
    } else {
      this._lineService.editLine(this.selectedLine, this.selectedLine.etag).subscribe(
        data => {
          //console.log(data);
          this.msgOk = "Izmene su sačuvane";
          this.isNewLine = false;
          this.getAllLines();
        },
        error => {
          //console.log(error);
          this.msgBad = "Greška";
        }
      );
    }
  }

  displayLineOnMap(lineId: any) {
    this._parent.DisplayLineOnMap(lineId, false, true);
  }

  selectRow(row: any){
    this.msgBad = null;
    this.msgOk = null;
    this._parent.RemoveAllLinesFromMap();
    this.isNewLine = false;
    if (row.Id == this.selectedLineId) {
      this.selectedLineId = "";
      this._parent.RemoveLineFromMap(row.Id);
      this._parent.displayOverlay();
      this._parent.DeselectBusStop();
    } else {
      if (this.selectedLineId.length > 0) {
        this._parent.RemoveLineFromMap(this.selectedLineId);
      }
      this.selectedLineId = row.Id;
      this.displayLineOnMap(this.selectedLineId);
      this._parent.removeOverlay();
    }
  }

  deleteLine(){
    this._lineService.deleteLine(this.selectedLine.Id)
      .subscribe(
        data => {
          //console.log(data);
          this.msgOk = "Linija "+this.selectedLine.Id+" je obrisana";
          this.isNewLine = false;
          this.getAllLines();
        },
        err => {
          //console.log(err);
          this.msgBad = "Greška";
        }
      );
  }

}