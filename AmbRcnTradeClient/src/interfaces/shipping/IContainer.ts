import { ContainerStatus } from "constants/app-constants";
import { IEntityCompany } from "interfaces/IEntity";
import { IIncomingStock } from "./../stockManagement/IIncomingStock";
export interface IContainer extends IEntityCompany {
    containerNumber: string;
    sealNumber: string;
    bookingNumber: string;
    bags: number;
    weighbridgeWeightKg: number; 
    status: ContainerStatus;
    id: string;
    name: string;
    companyId: string;
    stuffingDate: string | null;
    dispatchDate: string | null;
    stockWeightKg: number;
    incomingStocks: IIncomingStock[];
    nettWeightKg: number;
}
