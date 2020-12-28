import { IEntityCompany } from "interfaces/IEntity";
import { IPurchaseDetail } from "./IPurchaseDetail";

export interface IPurchase extends IEntityCompany {
    purchaseDetails: IPurchaseDetail[];
    id: string;
    name: string;
    companyId: string;
    purchaseNumber: number;
    supplierId: string;
    purchaseDate: string;
}
