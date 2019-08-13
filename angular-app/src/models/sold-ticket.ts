import { Guid } from "guid-typescript";
import { User } from './user';

export class SoldTicket {
    public Id: Guid;
    public Type: string;
    public UserId: Guid;
    public User: User;
    public Expires: Date;
    public Usages: number;
    public Price: number;
    public DateOfPurchase: Date;
}