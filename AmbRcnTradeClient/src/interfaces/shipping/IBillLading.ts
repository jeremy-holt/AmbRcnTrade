import { IContainer } from "./IContainer";

export interface IBillLading {
  notifyParty1Id: string;
  notifyParty2Id: string;
  consigneeId: string;
  blBodyText: string;
  shipperId: string;
  freightPrepaid: boolean;
  containersOnBoard: number;
  blDate: string | null;
  blNumber: string;
  containerIds: string[];
  id: string;
  name: string;
  companyId: string;
  containers: IContainer[];
  vesselId: string;

  shipperName?: string;
  consigneeName?: string;
  notifyParty1Name?: string;

  ownReferences: string;
  shipperReference: string;
  consigneeReference: string;
  
  destinationAgentId: string;


  shippingMarks: string;
  freightChargesPayableAt: string;
  bookingNumber: string;
  forwarderReference: string;

  selected?: boolean;
}
