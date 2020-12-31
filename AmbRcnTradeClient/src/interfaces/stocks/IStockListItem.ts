import { IInspection } from "./../inspections/IInspection";
import { IStockInfo } from "./IStockInfo";

export interface IStockListItem {
  stockIn: IStockInfo;
  stockOut: IStockInfo;
  locationId: string;
  lotNo: number;
  stockId: string;
  isStockIn: boolean;
  date: string;
  supplierName: string;
  supplierId: string;
  locationName: string;
  inspectionId: string;
  inspection: IInspection;
  origin: string;
  selected?: boolean;
}
