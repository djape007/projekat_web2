import { Guid } from "guid-typescript";
import { BusStopsOnLine } from './bus-stops-on-line';

export class BusStop {
    public Id: Guid;
    public X: number;
    public Y: number;
    public Name: string; //ovo je adresa. tako je na sajtu jgspns
    public Address: string; //nalaze se linije koje prolaze kroz tu stanicu
    public BusStopsOnLines: Array<BusStopsOnLine> = new Array();
    public etag: string;
}