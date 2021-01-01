import { IEntityCompany } from "interfaces/IEntity";
import { IAnalysisResult } from "./../inspections/IAnalysis";

export interface IStock extends IEntityCompany {
  id: string;
  name: string;
  companyId: string;
  locationId: string;
  locationName?: string;
  stockInDate: string | null;
  lotNo: number;
  bags: number;
  weightKg: number;
  inspectionId: string;
  stockOutDate: string | null;
  isStockIn: boolean;
  origin: string;
  analysisResult: IAnalysisResult,
  supplierId: string;
  supplierName?: string;

  selected?: boolean;
}
