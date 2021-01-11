import { IEntityCompany } from "interfaces/IEntity";
import { IAnalysisResult } from "./../inspections/IAnalysis";
import { IStuffingRecord } from "./IStuffingRecord";

export interface IStock extends IEntityCompany {
  locationId: string;
  analysisResult: IAnalysisResult;
  isStockIn: boolean;
  stockInDate: string | null;
  stockOutDate: string | null;
  bags: number;
  weightKg: number;
  lotNo: number;
  companyId: string;
  id: string;
  inspectionId: string;
  name: string;
  origin: string;
  supplierId: string;
  stuffingRecords: IStuffingRecord[];
  zeroedStock: boolean;
  selected?: boolean;
}
