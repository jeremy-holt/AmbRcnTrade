import { Currency } from "constants/app-constants";
import { IAnalysisResult } from "./../inspections/IAnalysis";

export interface IPurchaseListItem {
  id: string;
  supplierId: string;
  supplierName: string;
  purchaseNumber: number;
  purchaseDate: Date | string;
  quantityMt: number;
  purchaseDetails: IPurchaseDetailListItem[];
  bagsIn: number;
  bagsOut: number;
  balance: number;
}

export interface IPurchaseDetailListItem {
  stockIds: string[];
  currency: Currency;
  pricePerKg: number;
  priceAgreedDate: Date | string;
  stocks: IPurchaseDetailStockListItem[];
  analysisResult: IAnalysisResult;
  bagsIn: number;
  bagsOut: number;
  balance: number;
}

export interface IPurchaseDetailStockListItem {
  stockId: string;
  inspectionId: string;
  isStockIn: boolean;
  analysisResult: IAnalysisResult;
  bagsIn: number;
  bagsOut: number;
  balance: number;
}
