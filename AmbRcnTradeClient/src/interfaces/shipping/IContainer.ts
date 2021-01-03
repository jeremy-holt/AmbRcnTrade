import { IIncomingStock } from "./../stockManagement/IIncomingStock";
import { ContainerStatus } from "constants/app-constants";
import { IEntityCompany } from "interfaces/IEntity";
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
  dispatchDate:  string | null;
  stuffingWeightKg: number;
  incomingStocks: IIncomingStock[];
  nettWeightKg: number;
}
