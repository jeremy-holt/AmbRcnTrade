import { IStockInfo } from "interfaces/stocks/IStockInfo";

export interface IPurchaseListItem {
    supplierName: string;
    id: string;
    purchaseNumber: number;
    purchaseDate:  string;
    supplierId: string;
    stockIn: IStockInfo;
    stockOut: IStockInfo;
    stockBalance: IStockInfo;
}
