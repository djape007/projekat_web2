import { Bus } from './bus';
import { BusStopsOnLine } from './bus-stops-on-line';
import { PointPathLine } from './point-path-line';

export class Line {
    public Id: string;
    public Direction: string;
    public Buses: Array<Bus> = new Array();
    public BusStopsOnLines: Array<BusStopsOnLine> = new Array();
    public PointLinePaths: Array<PointPathLine> = new Array();
    public DisplayName: string;
    public etag: string;
}

