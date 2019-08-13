import { Guid } from "guid-typescript";
import { BusStop } from './bus-stop';
import { Line } from './line';

export class BusStopsOnLine {
    public Id: Guid;
    public BusStopId: Guid;
    public BusStop: BusStop;
    public LineId: string;
    public Line: Line;
}