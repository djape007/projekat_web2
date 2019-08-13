import { Guid } from "guid-typescript";
import { Line } from './line';

export class Timetable {
    public Id: Guid;
    public Times: string;
    public ValidFrom: Date;
    public LineId: string;
    public Line: Line;
}