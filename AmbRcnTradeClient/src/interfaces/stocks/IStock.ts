import { IEntityCompany } from "interfaces/IEntity";
import { IAnalysis } from "interfaces/inspections/IAnalysis";
import { IInspection } from "interfaces/inspections/IInspection";

export interface IStock extends IEntityCompany {
    id: string;
    name: string;
    companyId: string;
    locationId: string;
    stockInDate:  string | null;
    lotNo: number;
    bags: number;
    weightKg: number;
    inspectionIds: string[];
    stockOutDate: string | null;
    isStockIn: boolean;
    analysisResult: IAnalysis;
    inspections: IInspection[];
    origin: string;
    supplierId: string;
}
