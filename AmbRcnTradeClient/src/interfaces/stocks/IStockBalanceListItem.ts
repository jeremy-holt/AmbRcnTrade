import { IStockInfo } from "./IStockInfo";

export interface IStockBalanceListItem {
    lotNo: number;
    stockIn: IStockInfo;
    stockOut: IStockInfo;
    balance: IStockInfo;
    isStockZero: boolean;

}
