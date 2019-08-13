import { Guid } from "guid-typescript";
import { Pricelist } from './pricelist';
import { ProductType } from './product-type';

export class PriceHistory {
    public Id: Guid;
    public PricelistId: Guid;
    public Pricelist: Pricelist;
    public ProductTypeId: Guid;
    public ProductType: ProductType;
    public Price: number;
}