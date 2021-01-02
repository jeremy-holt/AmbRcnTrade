import { IAnalysisResult } from "interfaces/inspections/IAnalysis";

export interface IStockBalanceListItem {
    lotNo: number;
    bagsIn: number;
    bagsOut: number;
    balance: number;
    isStockZero: boolean;
    locationName: string;
    analysisResults: IAnalysisResult[];
    inspectionIds: string[];
    kor: number;
    moisture: number;
    count: number;
    weightKgIn: number;
    weightKgOut: number;
    balanceStockWeightKg: number;
}
