import { IAnalysis } from "./../inspections/IAnalysis";
import { IStockInfo } from "./IStockInfo";

export interface IStockListItem {
  stockIn: IStockInfo;
  stockOut: IStockInfo;
  analysisResult: IAnalysis;
  locationId: string;
  lotNo: number;
  stockId: string;
  isStockIn: boolean;
  date: string;
  supplierName: string;
  supplierId: string;
  locationName: string;
  origin: string;
  selected?: boolean;
}
