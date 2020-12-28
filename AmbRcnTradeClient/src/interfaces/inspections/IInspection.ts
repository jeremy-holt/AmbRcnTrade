import { Approval } from "constants/app-constants";
import { IEntityCompany } from "interfaces/IEntity";
import { IAnalysis } from "./IAnalysis";

export interface IInspection extends IEntityCompany {
    inspectionDate: string;
    inspector: string;
    lotNo: string;
    location: string;
    truckPlate: string;
    approxWeight: number;
    bags: number;
    id: string;
    name: string;
    companyId: string;
    analyses: IAnalysis[];
    approved: Approval;
    supplierId: string;
    analysisResult: IAnalysis;
}
