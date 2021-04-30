import { IEntityCompany } from "core/interfaces/IEntity";
import { IPurchaseDetail } from "./IPurchaseDetail";

export interface IPurchase extends IEntityCompany {
    purchaseDetails: IPurchaseDetail[];
    id: string;
    name: string;
    companyId: string;
    purchaseNumber: number;
    supplierId: string;
    purchaseDate: string;
    quantityMt: number;
    deliveryDate: string | null;
    value: number;
    valueUsd: number;
}
