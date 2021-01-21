import { IAnalysisResult } from "interfaces/inspections/IAnalysis";

export interface IStockListItem {
  companyId: string;
  purchaseId: string;
  locationId: string;
  bagsIn: number;
  bagsOut: number;
  lotNo: number;
  stockId: string;
  isStockIn: boolean;
  stockDate: string | null;
  locationName: string;
  origin: string;
  supplierName: string;
  supplierId: string;
  inspectionId: string;
  inspectionDate: string | null;
  analysisResult: IAnalysisResult;
  selected?: boolean;
  weightKgIn: number;
  weightKgOut: number;
  avgBagWeightKg: number;
  fiche: string;
}
