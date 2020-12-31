import { Currency } from "constants/app-constants";
import { IAnalysis } from "interfaces/inspections/IAnalysis";
import { IStockInfo } from "interfaces/stocks/IStockInfo";

export interface IPurchaseListItem {
  id: string;
  supplierId: string;
  supplierName: string;
  purchaseNumber: number;
  purchaseDate: Date | string;
  quantityMt: number;
  purchaseDetails: IPurchaseDetailListItem[];
  stockIn: IStockInfo;
  stockOut: IStockInfo;
  stockBalance: IStockInfo;
}

export interface IPurchaseDetailListItem {
  stockIds: string[];
  currency: Currency;
  pricePerKg: number;
  date: Date | string;
  stocks: IPurchaseDetailStockListItem[];
  analysisResult: IAnalysis;
  stockIn: IStockInfo;
  stockOut: IStockInfo;
  stockBalance: IStockInfo;
}

export interface IPurchaseDetailStockListItem {
  stockId: string;
  inspectionId: string;
  isStockIn: boolean;
  analysisResult: IAnalysis;
  stockIn: IStockInfo;
  stockOut: IStockInfo;
  stockBalance: IStockInfo;
}
