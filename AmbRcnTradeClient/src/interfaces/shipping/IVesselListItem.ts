export interface IVesselListItem {
  id: string;
  vesselName: string;
  eta: string | null;
  shippingCompanyName: string;
  forwardingAgentName: string;
  containersOnBoard: number;  
  companyId: string;
  numberBillsLading: number;
  selected?:boolean
}



