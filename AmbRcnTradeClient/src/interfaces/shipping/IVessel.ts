import { IBillLading } from "./IBillLading";

export interface IVessel {  
  containersOnBoard: number;
  forwardingAgentId: string;
  shippingCompanyId: string;
  billLadingIds: string[];
  companyId: string;
  id: string;
  name: string;
  portOfDestinationId: string;
  billLadings: IBillLading[];
  eta: string|null;
  vesselName: string;
  notes: string;
}

