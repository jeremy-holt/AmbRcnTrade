import { IAnalysisResult } from "interfaces/inspections/IAnalysis";
export interface IUnAllocatedStock {
    stockId: string;
    companyId: string;
    isStockIn: boolean;
    locationName: string;
    supplierName: string;
    lotNo: number;
    bags: number;
    analysisResult: IAnalysisResult;
    purchaseId: string;

    selected?:boolean;
}
