

export interface IBillLadingListItem {
  id: string;

  containersOnBoard: number;
  blDate: Date | string | null;
  blNumber: string;
  consigneeName: string;
  notifyParty1Name: string;
  notifyParty2Name: string;
  shipperName: string;
  portOfDestinationName: string;
  companyId: string;
}
