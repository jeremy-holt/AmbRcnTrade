import { IStockReference } from "./IStockReference";
import { Approval } from "constants/app-constants";

export interface IInspectionListItem {
  approved: Approval;
  inspectionDate: string;
  id: string;
  location: string;
  lotNo: string;
  inspector: string;
  bags: number;
  weightKg: number;
  truckPlate: string;
  supplierName: string;
  supplierId: string;
  kor: number;
  count: number;
  moisture: number;
  rejectsPct: number;
  stockReferences: IStockReference[];
  stockAllocations: number;
  unallocatedBags: number;
  unallocatedWeightKg: number;
  avgBagWeightKg:number;
  css?: string;
  price: number;
  warehouseName: string;
  warehouseId: string;
  fiche: string;
  origin: string;
}
