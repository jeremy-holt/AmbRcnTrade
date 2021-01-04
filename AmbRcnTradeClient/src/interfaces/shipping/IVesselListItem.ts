export interface IVesselListItem {
  id: string;
  vesselName: string;
  eta: string | null;
  containersOnBoard: number;
  blDate: string | null;
  blNumber: string;
  shippingCompany: string;
  forwardingAgent: string;
}
