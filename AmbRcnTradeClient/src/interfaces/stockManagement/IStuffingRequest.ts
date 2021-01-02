import { IIncomingStock } from "./IIncomingStock";


export interface IStuffingRequest {
  containerId: string;
  stuffingDate: string;
  incomingStocks: IIncomingStock[];
}
