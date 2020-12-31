import { Currency } from "constants/app-constants";
import { IStockInfo } from "interfaces/stocks/IStockInfo";

export interface IPurchaseListItem {
  id: string;
  supplierId: string;
  supplierName: string;
  purchaseNumber: number;
  purchaseDate: string;
  stockIn: IStockInfo;
  stockOut: IStockInfo;
  stockBalance: IStockInfo;
  pricePerKg: number;
  currency: Currency;
  exchangeRate: number;
  quantityMt: number;
}
