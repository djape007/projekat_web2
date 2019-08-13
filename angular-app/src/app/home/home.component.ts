import { Component, ViewChild, OnInit, Input, NgZone } from '@angular/core';
import {} from 'googlemaps';
import { Router, RoutesRecognized } from '@angular/router';
import { Line } from 'src/models/line';
import { BusStop } from 'src/models/bus-stop';
import { AuthService } from '../services/auth.service';
import { LineService } from '../services/line.service';
import { Bus } from 'src/models/bus';
import { BusService } from '../services/bus.service';
import { _MatChipListMixinBase } from '@angular/material';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})

export class HomeComponent implements OnInit {
  @ViewChild('map') mapElement: any;
  @ViewChild('leftPanel') leftPanelComponent: any;
  @ViewChild('rightPanel') rightPanelComponent: any;
  map: google.maps.Map;
  putanjePrikazanihLinija: Array<any> = new Array<any>();
  stanicePrikazaneNaMapi: Array<any> = new Array<any>();
  prikazaneLinije: Array<any> = new Array<any>(); //lineId => Line objekat
  prikazaniAutobusi: Array<any> = new Array<any>();
  isConnectedWS: Boolean = false;

  prikazaneStaniceJedneLinije: Array<any> = new Array<any>();

  readonly busStopIcon: string = "../../assets/imgs/busStopMarker_mini.png";
  readonly busStopIconSelected: string = "../../assets/imgs/busStopMarker_mini_selected.png";

  editSelectedMarker: google.maps.Marker = null;
  editSelectedBusStop: BusStop = null;
  editSelectedLine: Line = null;
  editSelectedLinePath: google.maps.Polyline = null;

  public displayedPanel: string = 'none';

  constructor(private _router: Router, private _auth: AuthService,private _lineService: LineService,
    private _busService: BusService,private ngZone: NgZone) { }

  ngOnInit(): void {
    this._router.events.subscribe(event => {
      if (event instanceof RoutesRecognized) {
        if(event.url == '/home')
          this.prikaziMapu();
      }
    });

    this.InitMap();
    this.ConnectWS();
 }

private ConnectWS() {
  this.checkConnection();
  this.subscribeForBusPositions();
}

private checkConnection(){
  this._busService.startConnection().subscribe(e => {
    this.isConnectedWS = e;
  });
}

private subscribeForBusPositions () {
  this._busService.registerBusesLocations().subscribe(
    data => {
      data.forEach(bus => {
        this.DrawBusOnMap(bus);
      });
    }
  )
}

 private InitMap() {
  const mapProperties = {
      center: new google.maps.LatLng(45.248636, 19.833549),
      zoom: 14,
      mapTypeId: google.maps.MapTypeId.ROADMAP,
      disableDefaultUI: true,
      mapTypeControlOptions: {
      mapTypeIds: ['styled_map']
    }
  };

  var styledMap = new google.maps.StyledMapType([ { "elementType": "geometry", "stylers": [{ "color": "#ebe3cd" }] }, { "elementType": "labels.text.fill", "stylers": [{ "color": "#523735" }] }, { "elementType": "labels.text.stroke", "stylers": [{ "color": "#f5f1e6" }] }, { "featureType": "administrative", "elementType": "geometry.stroke", "stylers": [{ "color": "#c9b2a6" }] }, { "featureType": "administrative.land_parcel", "elementType": "geometry.stroke", "stylers": [{ "color": "#dcd2be" }] }, { "featureType": "administrative.land_parcel", "elementType": "labels.text.fill", "stylers": [{ "color": "#ae9e90" }] }, { "featureType": "landscape.natural", "elementType": "geometry", "stylers": [{ "color": "#dfd2ae" }] }, { "featureType": "poi", "elementType": "geometry", "stylers": [{ "color": "#dfd2ae" }] }, { "featureType": "poi", "elementType": "labels.text.fill", "stylers": [{ "color": "#93817c" }] }, { "featureType": "poi.park", "elementType": "geometry.fill", "stylers": [{ "color": "#a5b076" }] }, { "featureType": "poi.park", "elementType": "labels.text.fill", "stylers": [{ "color": "#447530" }] }, { "featureType": "road", "elementType": "geometry", "stylers": [{ "color": "#f5f1e6" }] }, { "featureType": "road.arterial", "elementType": "geometry", "stylers": [{ "color": "#fdfcf8" }] }, { "featureType": "road.highway", "elementType": "geometry", "stylers": [{ "color": "#f8c967" }] }, { "featureType": "road.highway", "elementType": "geometry.stroke", "stylers": [{ "color": "#e9bc62" }] }, { "featureType": "road.highway.controlled_access", "elementType": "geometry", "stylers": [{ "color": "#e98d58" }] }, { "featureType": "road.highway.controlled_access", "elementType": "geometry.stroke", "stylers": [{ "color": "#db8555" }] }, { "featureType": "road.local", "elementType": "labels.text.fill", "stylers": [{ "color": "#806b63" }] }, { "featureType": "transit.line", "elementType": "geometry", "stylers": [{ "color": "#dfd2ae" }] }, { "featureType": "transit.line", "elementType": "labels.text.fill", "stylers": [{ "color": "#8f7d77" }] }, { "featureType": "transit.line", "elementType": "labels.text.stroke", "stylers": [{ "color": "#ebe3cd" }] }, { "featureType": "transit.station", "elementType": "geometry", "stylers": [{ "color": "#dfd2ae" }] }, { "featureType": "transit.station.bus", "stylers": [{ "visibility": "off" }] }, { "featureType": "water", "elementType": "geometry.fill", "stylers": [{ "color": "#b9d3c2" }] }, { "featureType": "water", "elementType": "labels.text.fill", "stylers": [{ "color": "#92998d" }] }], {name: "styled_map"});

  this.map = new google.maps.Map(this.mapElement.nativeElement, mapProperties);
  this.map.mapTypes.set('styled_map', styledMap);
  this.map.setMapTypeId('styled_map');
 }

 public removeOverlay(){
  let el = document.getElementsByClassName('map-overlay')[0];
  if (el != null) {
    el.className = "map-no-overlay";
    this.mapElement.nativeElement.className = "";
  }
 }

 public displayOverlay() {
  let el = document.getElementsByClassName('map-no-overlay')[0];
  if (el != null) {
    el.className = "map-overlay";
    this.mapElement.nativeElement.className = "blurGrayscale";
  }
 }

 public checkIfLoggedIn(): Boolean{
    if(this._auth.getToken())
      return true;
    else
      return false;
  }

 public prikaziLeviMeni() {
  this.hideRightPanel();
  this.displayLeftPanel();

  this.moveMapRight();
  this.displayOverlay();
 }

 public prikaziDesniMeni() {
  this.hideLeftPanel();
  this.displayRightPanel();
  
  this.moveMapLeft();
  this.displayOverlay();
 }

 public prikaziMapu() {
  this.hideLeftPanel();
  this.hideRightPanel();
  this.centerMap();
  this.removeOverlay();
 }

 private NavigateToMap() {
  this._router.navigate(['/home']);
 }

  private displayLeftPanel() {
    let levi = document.getElementById('leftPanel');
    levi.style.webkitTransform = "translate3d(0,0,0)";
    levi.style.zIndex = "100";
  }

  private hideLeftPanel() {
    let levi = document.getElementById('leftPanel');
    levi.style.webkitTransform = "translate3d(-400px,0,0)";
    levi.style.zIndex = "-500";
  }

  private displayRightPanel() {
    let desni = document.getElementById('rightPanel');
    desni.style.zIndex = "100";
    desni.style.webkitTransform = "translate3d(0,0,0)";
  }

  private hideRightPanel() {
    let desni = document.getElementById('rightPanel');
    desni.style.webkitTransform = "translate3d(400px,0,0)";
    desni.style.zIndex = "-500";
  }

  private moveMapRight() {
    let mapHolder = document.getElementById('map-holder');
    mapHolder.style.webkitTransform = "translate3d(400px,0,0)";
  }

  private moveMapLeft() {
    let mapHolder = document.getElementById('map-holder');
    mapHolder.style.webkitTransform = "translate3d(-400px,0,0)";
  }

  private centerMap() {
    let mapHolder = document.getElementById('map-holder');
    mapHolder.style.webkitTransform = "translate3d(0,0,0)";
  }

  public RemoveAllLinesFromMap() {
    Object.keys(this.prikazaneLinije).forEach((item) => {
      this.RemoveLineFromMap(item);
    });
  }

  public DisplayLineOnMap(lineId: string, busStopSelectable: boolean = false, linePathEditable: boolean = false) {
    if (! this.isConnectedWS) {
      //this.ConnectWS();
      //ne radi. poveze se, ali server ne vraca podatke
      //a kad je gore u OnInit onda sve radi i server vraca podatke
    }

    this._lineService.getLine(lineId).subscribe(
      (data) => {
        console.log(data.body);
        this.DrawLineOnMap(data.body, busStopSelectable, linePathEditable);
      }, 
      (error) => {
        console.log(error);
      })
  }

  public DrawLineOnMap(linija: Line, busStopSelectable: boolean = false, linePathEditable: boolean = false) {
    if (linija.Id in this.putanjePrikazanihLinija) {
      console.log("Linija je vec nacrtana");
      return;
    }

		let SelectedLineCoordinates = new Array<google.maps.LatLng>();
    
    if (linija.PointLinePaths.length == 0) {
      console.log(linija.Id + " nema putanju");
      return;
    }

    let sortiranaPutanja = linija.PointLinePaths.sort((a,b) => a.SequenceNumber - b.SequenceNumber);
    
		sortiranaPutanja.forEach((item) => {
      SelectedLineCoordinates.push(new google.maps.LatLng(item.X,item.Y));	
		});
    
    let bojaLinije = "#"+linija.Id+linija.Id;

    if (bojaLinije.length == 5) {
      bojaLinije += "55";
    }

		let polyOptions = {
            path: SelectedLineCoordinates,
            geodesic: true,
						strokeColor: bojaLinije,
						strokeOpacity: 1,
            strokeWeight: 4,
            editable: (linePathEditable ? true : false) //za admina
    }
		
		this.putanjePrikazanihLinija[linija.Id] = new google.maps.Polyline(polyOptions);
    this.putanjePrikazanihLinija[linija.Id].setMap(this.map);
    this.prikazaneLinije[linija.Id] = linija;

    if (linePathEditable) {
      this.editSelectedLine = linija;
      this.editSelectedLinePath = this.putanjePrikazanihLinija[linija.Id];
    } else {
      this.editSelectedLine = null;
      this.editSelectedLinePath = null;
    }

    if (linija.BusStopsOnLines.length > 0) {
      this.prikazaneStaniceJedneLinije = new Array<any>();
      linija.BusStopsOnLines.forEach((BusStopLineConnection, index) => {
          this.DrawBusStopOnMap(BusStopLineConnection.BusStop, linija.Id, busStopSelectable);
      });
    } else {
      console.log(linija.Id + " nema stanice");
    }

  }

  public RemoveLineFromMap(lineId: string) {
    if (!(lineId in this.prikazaneLinije)) {
      console.log("JAOOO " + lineId + " bolje ovo da ne vidim");
      return;
    }

    if (this.editSelectedLine != null && lineId == this.editSelectedLine.Id) {
      this.editSelectedLine = null;
      this.editSelectedLinePath = null;
    }

    let linijaZaBrisanje = this.prikazaneLinije[lineId] as Line;
    this.putanjePrikazanihLinija[lineId].setMap(null);
    
    linijaZaBrisanje.BusStopsOnLines.forEach(element => {
      let kljuc = element.BusStopId.toString() + "_" + lineId;
      if (kljuc in this.stanicePrikazaneNaMapi) {
        this.stanicePrikazaneNaMapi[kljuc].setMap(null);
      }
    });

    linijaZaBrisanje.Buses.forEach(element => {
      this.RemoveBusFromMap(element);
    });
    delete this.putanjePrikazanihLinija[lineId];
    delete this.prikazaneLinije[lineId]
  }

  public DrawBusStopOnMap(busStop: BusStop, lineId: string = "", isAdmin: boolean =false) {
    let markerIconPath = this.busStopIcon;

    let kljucStanice = busStop.X.toFixed(6) +"_"+ busStop.Y.toFixed(6);
    if (kljucStanice in this.prikazaneStaniceJedneLinije) {
      console.log("Duplikat");
      return;
    }

    if (isAdmin) {
      if (this.editSelectedMarker !== null) {
        this.editSelectedMarker.setIcon(markerIconPath);
      }
      this.editSelectedBusStop = busStop;
      markerIconPath = this.busStopIconSelected;
    }

    let marker = this.DrawMarkerOnMap(busStop.X, busStop.Y, busStop.Name + "|" + busStop.Address, markerIconPath, isAdmin);
    let infoWindow = new google.maps.InfoWindow();

    marker.addListener('click', () => {
      let timeString = 'Trenutno nema autobusa koji treba da stignu.';
      let timesInSec = [];

      if (! isAdmin) {
        timesInSec = this.calculateTimesForArrivingBuses(busStop,lineId);
      }

      if(timesInSec.length != 0){
        let sortedtimes = timesInSec.sort((x,y)=> (Number)(x) - (Number)(y));
        let closestTime = sortedtimes[0];
        let minutes = Math.floor((Number)(closestTime) / 60);
        let seconds = Math.round((Number)(closestTime) % 60);
        timeString = `<div>Autobus stize za <b>${minutes} min : ${seconds} sec<b></div>`;
      }
      
      if (this.editSelectedMarker == marker || !isAdmin) {
        let prikaziLinijeHTML = "";
        let content = "";

        content += `<div><b>${busStop.Name}</b></div>`;
        if(this._auth.getRole() == "Admin")
          content += `<div>${busStop.Id}</div>`;
        content += `<div>${busStop.Address}</div>
        ${prikaziLinijeHTML}`;
        infoWindow.setContent(`${content}${timeString}`);
        infoWindow.open(this.map, marker);
      }

      if (isAdmin && this.editSelectedMarker !== marker) {
        this.SelectBusStop(marker, busStop);
      }
    });

    if (isAdmin) {
      this.SelectBusStop(marker, busStop);
    }

    this.stanicePrikazaneNaMapi[busStop.Id.toString() + "_" + lineId] = marker;
    this.prikazaneStaniceJedneLinije[kljucStanice] = '1';
  }

  private SelectBusStop(newBusStopMarker, busStop) {
    if (this.editSelectedMarker !== null ) {
      this.editSelectedMarker.setIcon(this.busStopIcon);
    }
    this.editSelectedMarker = newBusStopMarker;
    this.editSelectedBusStop = busStop;
    newBusStopMarker.setIcon(this.busStopIconSelected);
  }

  public DeselectBusStop() {
    if (this.editSelectedMarker !== null ) {
      this.editSelectedMarker.setIcon(this.busStopIcon);
    }
    this.editSelectedMarker = null;
    this.editSelectedBusStop = null;
  }

  private CreateInfoWindow(content: string): google.maps.InfoWindow {
    let infowindow = new google.maps.InfoWindow({
      content: content
    });
    return infowindow;
  }

  private DrawMarkerOnMap(latX: number, lngY: number, title: string, customIcon:string = "", editable: boolean = false): google.maps.Marker {
    let marker: google.maps.Marker = null;
    if (customIcon.trim().length > 0) {
      marker = new google.maps.Marker({
        position: new google.maps.LatLng(latX, lngY),
        map: this.map,
        //title: title,
        icon: customIcon,
        //draggable: true //za admina
      });
    } else {
      marker = new google.maps.Marker({
        position: new google.maps.LatLng(latX, lngY),
        map: this.map,
        title: title
      });
    }

    if (editable) {
      marker.setDraggable(true);
    }

    marker.setMap(this.map);

    return marker;
  }

  private RemoveMarkerFromMap(marker: google.maps.Marker) {
    marker.setMap(null);
  }

  private DrawBusOnMap(bus:Bus) {
    if ((bus.LineId in this.prikazaneLinije)) {
      if (bus.Id in this.prikazaniAutobusi) {
        var linija = this.prikazaneLinije[bus.LineId];
        if(linija.Buses.find(x => x.Id == bus.Id) == null){
          linija.Buses.push(bus);
        }
        this.prikazaniAutobusi[bus.Id].setTitle(bus.Id);
        this.prikazaniAutobusi[bus.Id].setZIndex(120);
        this.prikazaniAutobusi[bus.Id].setPosition(
          new google.maps.LatLng(bus.X, bus.Y)
        );
      } else {
        let markerIconPath = "../../assets/imgs/busMarker.png";
        let busMarker = this.DrawMarkerOnMap(bus.X, bus.Y, bus.Id + "|" + bus.LineId, markerIconPath) as google.maps.Marker;
        busMarker.setZIndex(120);
        busMarker.addListener('click', () => {
          let content =
            `<div>Registracija: <b>${bus.Id}</b></div>
            <div>Trenutna linija: ${bus.LineId}</div>`;
          var infoWindow = new google.maps.InfoWindow();
          infoWindow.setContent(content);
          infoWindow.open(this.map, busMarker);
        });
        this.prikazaniAutobusi[bus.Id] = busMarker;
      }
    } else if (bus.Id in this.prikazaniAutobusi) {
      this.RemoveBusFromMap(bus);
    }
  }

  private RemoveBusFromMap(bus:Bus) {
    if (bus.Id in this.prikazaniAutobusi) {
      this.prikazaniAutobusi[bus.Id].setMap(null);
      delete this.prikazaniAutobusi[bus.Id];
    }
  }

  sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
  }

  calculateTimeInSecs(stationLat: number, stationLng: number, busLat: number, busLng: number, betweenPoints: Array<any>) : number {
    var path = new Array<google.maps.LatLng>();
    betweenPoints.forEach(element => {
      path.push(new google.maps.LatLng(element.X, element.Y));
    });
    var len = path.length;
    var stationLocation = new google.maps.LatLng(stationLat, stationLng);
    var busLocatioon = new google.maps.LatLng(busLat, busLng);
    path[0] = busLocatioon;
    path[len-1] = stationLocation; 
    var polyline = new google.maps.Polyline({
      path: path,
      // strokeColor: "#ff0000",
      // strokeOpacity: 0.6,
      // strokeWeight: 5
    });
    var distance = google.maps.geometry.spherical.computeLength(polyline.getPath());
    //polyline.setMap(this.map);
    var speedKm_h = 50;
    var speedM_s = speedKm_h*(1000/3600);
    var time = distance/speedM_s;
    return time;
  }

  calculateTimesForArrivingBuses(busStop: BusStop, lineId: string){
    var timesInSec = new Array<Number>();
    var distanceStationArr = new Array<any>();
    var linija = this.prikazaneLinije[lineId];
    linija.PointLinePaths.forEach(element => {
      const pointLocation = new google.maps.LatLng(element.X, element.Y);
      const busStopLocatioon = new google.maps.LatLng(busStop.X, busStop.Y);
      const dist = google.maps.geometry.spherical.computeDistanceBetween(pointLocation, busStopLocatioon);
      distanceStationArr.push({'dist' : dist, 'point' : element});
    });
    var sortedStationDist = distanceStationArr.sort((x,y)=> (Number)(x.dist) - (Number)(y.dist));
    var closestStationPoint = sortedStationDist[0];
    
    if(linija.Buses==null || linija.Buses.length == 0)
      return timesInSec;

    linija.Buses.forEach(element => {
      var bus = this.prikazaniAutobusi[element.Id];
      if(bus == null)
        return timesInSec;
      var distanceBusArr = new Array<any>();
      linija.PointLinePaths.forEach(element => {
        const pointLocation = new google.maps.LatLng(element.X, element.Y);
        const busLocatioon = new google.maps.LatLng(bus.position.lat(), bus.position.lng());
        const dist = google.maps.geometry.spherical.computeDistanceBetween(pointLocation, busLocatioon);
        distanceBusArr.push({'dist' : dist, 'point' : element});
      });
      var sortedBusDist = distanceBusArr.sort((x,y)=> (Number)(x.dist) - (Number)(y.dist));
      var closestBusPoint = sortedBusDist[0];
      if(linija.Id == "22B" || linija.Id == "7A" || linija.Id == "8A" || linija.Id == "4A" 
      || linija.Id == "9A" || linija.Id == "17A" || linija.Id == "23B" ){
        if(closestBusPoint.point.SequenceNumber >= closestStationPoint.point.SequenceNumber){
          var betweenPoints : Array<any> = linija.PointLinePaths.filter(x => x.SequenceNumber <= closestBusPoint.point.SequenceNumber
            && x.SequenceNumber >= closestStationPoint.point.SequenceNumber);
          var betweenPointsReversed = betweenPoints.reverse();
          timesInSec.push(this.calculateTimeInSecs(busStop.X, busStop.Y, bus.position.lat(), bus.position.lng(), betweenPointsReversed));
        }
      }
      else {
        if(closestBusPoint.point.SequenceNumber <= closestStationPoint.point.SequenceNumber){
          var betweenPoints : Array<any> = linija.PointLinePaths.filter(x => x.SequenceNumber >= closestBusPoint.point.SequenceNumber
            && x.SequenceNumber <= closestStationPoint.point.SequenceNumber);
          timesInSec.push(this.calculateTimeInSecs(busStop.X, busStop.Y, bus.position.lat(), bus.position.lng(), betweenPoints));
        }
      }
    });
    
    return timesInSec;
  }

  getPrikazaneLinije(): Array<any>{
    return this.prikazaneLinije;
  }

  drawPoint(path: google.maps.LatLng[]){
    var polyline = new google.maps.Polyline({
      path: path,
      strokeColor: "#ff0000",
      strokeOpacity: 1,
      strokeWeight: 8,
    });
    //var distance = google.maps.geometry.spherical.computeLength(polyline.getPath());
    polyline.setMap(this.map);
  }
}
