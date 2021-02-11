import { IIncomingStock } from "./../stockManagement/IIncomingStock";
import { ContainerStatus, Teu } from "constants/app-constants";
import { IEntityCompany } from "interfaces/IEntity";
export interface IContainer extends IEntityCompany {
  containerNumber: string;
  sealNumber: string;
  exporterSealNumber: string;
  bookingNumber: string;
  bags: number;
  weighbridgeWeightKg: number;
  status: ContainerStatus;
  id: string;
  name: string;
  companyId: string;
  dispatchDate: string | null;
  stuffingWeightKg: number;
  incomingStocks: IIncomingStock[];
  nettWeightKg: number;
  stuffingDate: string | null;
  vgmTicketNumber: string;
  teu: Teu;
  vesselId: string;
  vesselName?: string;
  selected?: boolean;
}
