import { IDocument } from "./IDocument";
import { Teu } from "constants/app-constants";
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
  portOfDestinationId: string;
  portOfDestinationName: string;
  portOfLoadingId: string;

  shippingMarks: string;
  forwarderReference: string;
  productDescription: string;
  preCargoDescription: ICargoDescription;

  numberPackagesText: string;
  nettWeightkgText: string;
  grossWeightKgText: string;
  vgmWeightKgText: string;

  numberBags: number | null;
  nettWeightKg: number | null;
  grossWeightKg: number | null;

  oceanFreight: string;
  oceanFreightPaidBy: string;
  freightOriginCharges: string;
  freightOriginChargesPaidBy: string;
  freightDestinationCharge: string;
  freightDestinationChargePaidBy: string;

  teu: Teu;

  selected?: boolean;
  documents: IDocument[];
}

export interface ICargoDescription {
  header: string;
  footer: string;
}
