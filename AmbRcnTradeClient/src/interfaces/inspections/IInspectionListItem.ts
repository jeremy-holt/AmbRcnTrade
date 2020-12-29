import { Approval } from "constants/app-constants";

export interface IInspectionListItem {
  approved: Approval;
  inspectionDate: string;
  id: string;
  location: string;
  lotNo: string;
  inspector: string;
  bags: number;
  truckPlate: string;
  supplierName: string;
  supplierId: string;
  kor: number;
  count: number;
  moisture: number;
  rejectsPct: number;
  css?: string;
}
