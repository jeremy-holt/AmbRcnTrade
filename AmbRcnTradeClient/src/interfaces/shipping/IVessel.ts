import { IContainer } from "interfaces/shipping/IContainer";
import { IEtaHistory } from "./IEtaHistory";

export interface IVessel {
  etaHistory: IEtaHistory[];
  shippingCompany: string;
  forwardingAgent: string;
  blDate: string | null;
  blNumber: string;
  containerIds: string[];
  containers: IContainer[];
  containersOnBoard: number;
  id: string;
  name: string;
  companyId: string;
  notifyParty1: string;
  notifyParty2: string;
  consignee: string;
  blBodyText: string;
  freightPrepaid: boolean;
}

