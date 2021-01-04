import { IContainer } from "interfaces/shipping/IContainer";
import { IEtaHistory } from "./IEtaHistory";

export interface IVessel {
  etaHistory: IEtaHistory[];
  shippingCompany: string;
  forwardingAgent: string;
  blDate: string | null;
  blNumber: string;
  containerIds: string[];
  containersOnBoard: number;
  id: string;
  name: string;
  companyId: string;
}

export interface IVesselDto extends IVessel{
  containers:IContainer[];
}
