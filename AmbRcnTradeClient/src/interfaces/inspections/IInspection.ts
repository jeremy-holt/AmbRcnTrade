import { IEntityCompany } from "interfaces/IEntity";
import { IAnalysis, IAnalysisResult } from "./IAnalysis";
import { IStockReference } from "./IStockReference";

export interface IInspection extends IEntityCompany {
  inspectionDate: string;
  inspector: string;
  lotNo: string;
  location: string;
  truckPlate: string;
  bags: number;
  id: string;
  name: string;
  companyId: string;
  analyses: IAnalysis[];
  supplierId: string;
  analysisResult: IAnalysisResult;
  stockReferences: IStockReference[];
}
