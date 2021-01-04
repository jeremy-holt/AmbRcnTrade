import { IBillLading } from "./IBillLading";
import { IEtaHistory } from "./IEtaHistory";

export interface IVessel {
  etaHistory: IEtaHistory[];
  containersOnBoard: number;
  forwardingAgentId: string;
  shippingCompanyId: string;
  billLadingIds: string[];
  companyId: string;
  id: string;
  name: string;
  portOfDestinationId: string;
  billLadings: IBillLading[];
}

