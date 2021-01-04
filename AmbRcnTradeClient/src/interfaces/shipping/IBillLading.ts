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

  selected?: boolean;
}
