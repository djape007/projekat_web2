import { Guid } from "guid-typescript";
import { PriceHistory } from './price-history';

export class ProductType {
    public Id: Guid;
    public Name: string;
    public PriceHistories: Array<PriceHistory> = new Array();
}