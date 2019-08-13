import { Component, OnInit, Inject, forwardRef } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Timetable } from 'src/models/timetable';
import { HomeComponent } from '../home/home.component';
import { TimetableService } from '../services/timetable.service';
import { MatTableDataSource } from '@angular/material';

@Component({
  selector: 'app-timetable',
  templateUrl: './timetable.component.html',
  styleUrls: ['./timetable.component.css']
})
export class TimetableComponent implements OnInit {
  dataSource = new MatTableDataSource();
  displayedColumns: string[] = ['Id', 'Direction'];

  selectedRowIndex: number = -1;
  selectedRowElements: Array<any> = new Array<any>();
  timetables: Array<Timetable>;

  constructor(@Inject(forwardRef(() => HomeComponent)) private _parent: HomeComponent,
     private _sevice: TimetableService, private _formBuilder: FormBuilder) { }

  ngOnInit() {
    this._parent.prikaziLeviMeni();
    this.getAllTimetables();
  }

  getAllTimetables(){
    this._sevice.getAllTimetables()
    .subscribe(
      data => {
        this.timetables = data;
        this.dataSource = new MatTableDataSource(this.createDataSource(data, this._parent.getPrikazaneLinije()));
      },
      err => {
        console.log(err);
      }
    )
  }

  selectLine(row: any){
    this.selectedRowIndex = row.Index;
    if(row.Selected)
    {
      row.Selected = false;
      this.removeLineFromMap(row.Id);
      var index = this.selectedRowElements.findIndex(x => x.LineId == row.Id);
      this.selectedRowElements.splice(index,1)
    }else
    {
      row.Selected = true;
      this.displayLineOnMap(row.Id);
      this.selectedRowElements.push(this.timetables.find(x => x.LineId == row.Id));
    }
  }

  displayLineOnMap(lineId: any) {
    this._parent.DisplayLineOnMap(lineId);
  }

  removeLineFromMap(lineId: any){
    this._parent.RemoveLineFromMap(lineId);
  }

  createDataSource(data: any, prikazaneLinije: any[]): any{
    var retVal = new Array();
    for(let item of data){
      var pushVal = {
        Selected: false,
        Id : item.LineId,
        Direction : item.Line.Direction,
        LineNumber : (Number)(item.LineId.replace('A','').replace('B','').replace('A','').replace('B',''))
      };

      if(prikazaneLinije[pushVal.Id]){
        pushVal.Selected = true;
        this.selectedRowElements.push(this.timetables.find(x => x.LineId == pushVal.Id));
      }

      retVal.push(pushVal);      
    }
    return retVal.sort((x,y) => x.LineNumber - y.LineNumber);
  }

  applyFilter(filterValue: string) {
    if(filterValue.toLowerCase().trim().includes('prika'))
      this.dataSource.filter = "true";
    else
      this.dataSource.filter = filterValue.trim().toLowerCase();
  }

  getTimesJson(timetable: Timetable, pickedDay: string): any{
    let timesJson = JSON.parse(timetable.Times);
    let selectedDayTimesJson = new Array<string>();
    if(pickedDay == "Radni dan")
      selectedDayTimesJson = timesJson['Radni_dan'];
    else if(pickedDay == "Subota")
      selectedDayTimesJson = timesJson['Subota'];
    else if(pickedDay == "Nedelja")
      selectedDayTimesJson = timesJson['Nedelja'];

    var timetableJson = new Array<string>();

    let polasciPoSatima = {};
    selectedDayTimesJson.forEach(
      (item, indeks) => {
        let satMinut = item.split(":");
        if (!(satMinut[0] in polasciPoSatima)) {
          polasciPoSatima[satMinut[0]] = new Array<string>();
        }

        polasciPoSatima[satMinut[0]].push(satMinut[1]);
      });
    
    for (let sat = 4; sat < 24+4; sat++) {
      let satStr = (sat % 24).toString().padStart(2, '0');
      if (satStr in polasciPoSatima) {
        polasciPoSatima[satStr] = polasciPoSatima[satStr].sort((x,y) => (Number)(x) - (Number)(y));
        let prikazJednogSata = satStr+"|" + polasciPoSatima[satStr].join(",");
        timetableJson.push(prikazJednogSata);
      }
    }

    return timetableJson;
  }

  setSelectedDay(timetable: any,selected_day: string){
    timetable['SelectedDay'] = selected_day;
  }
}  

