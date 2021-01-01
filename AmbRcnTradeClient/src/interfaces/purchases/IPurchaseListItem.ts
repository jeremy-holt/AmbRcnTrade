import { Currency } from "constants/app-constants";
import { IStockInfo } from "interfaces/stocks/IStockInfo";
import { IAnalysisResult } from "./../inspections/IAnalysis";

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
  analysisResult: IAnalysisResult;
  stockIn: IStockInfo;
  stockOut: IStockInfo;
  stockBalance: IStockInfo;
}

export interface IPurchaseDetailStockListItem {
  stockId: string;
  inspectionId: string;
  isStockIn: boolean;
  analysisResult: IAnalysisResult;
  stockIn: IStockInfo;
  stockOut: IStockInfo;
  stockBalance: IStockInfo;
}
