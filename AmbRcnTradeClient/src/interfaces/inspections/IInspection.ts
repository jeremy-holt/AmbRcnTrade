import { IEntityCompany } from "interfaces/IEntity";
import { IAnalysis } from "./IAnalysis";

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
    analysisResult: IAnalysis;
    stockReferences: IStockReference[];
}
