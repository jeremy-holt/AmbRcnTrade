import { Approval } from "constants/app-constants";

export interface IInspectionListItem {
    approved: Approval;
    inspectionDate: string;
    inspectionId: string;
    location: string;
    lotNo: string;
    inspector: string;
    bags: number;    
    truckPlate: string;
    supplierName: string;
    supplierId: string;
}
