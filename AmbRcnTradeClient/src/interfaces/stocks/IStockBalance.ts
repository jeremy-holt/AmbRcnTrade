import { IAnalysisResult } from "interfaces/inspections/IAnalysis";


export interface IStockBalance {
  lotNo: number;

  bagsIn: number;
  bagsOut: number;
  balance: number;
  locationName: string;
  locationId: string;  
  analysisResults: IAnalysisResult[];
  kor: number;
  moisture: number;
  count: number;
  weightKgIn: number;
  weightKgOut: number;
  balanceWeightKg: number;
  avgBagWeightKg: number;
  supplierName: string;
  supplierId: string;  
  selected?: boolean;
}
