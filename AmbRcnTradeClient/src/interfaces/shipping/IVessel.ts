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
  portOfLoadingId: string;
  billLadings: IBillLading[];
  eta: string|null;
  vesselName: string;
  voyageNumber: string;
  serviceContract: string;
  bookingNumber:string;
  notes: string;
}

