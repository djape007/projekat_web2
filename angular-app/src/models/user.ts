import { Guid } from "guid-typescript";
import { SoldTicket } from './sold-ticket';

export class User {
    public Id: Guid;
    public Role: string;
    public Status: string;
    public Type: string;
    public Password: string;
    public Email: string;
    public Name: string;
    public Surname: string;
    public Dob: Date;
    public HasDocument: boolean;
    public Address: string;
    public Files: string;
    public SoldTickets: Array<SoldTicket> = new Array();
}