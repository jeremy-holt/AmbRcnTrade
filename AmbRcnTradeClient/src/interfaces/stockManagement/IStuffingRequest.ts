import { ContainerStatus } from "constants/app-constants";
import { IStockBalance } from "interfaces/stocks/IStockBalance";


export interface IStuffingRequest {
  containerId: string;
  stuffingDate: string;
  stockBalance: IStockBalance;
  bags: number;
  weightKg: number;
  status: ContainerStatus
}
