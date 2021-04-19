import { IStockBalance } from "interfaces/stocks/IStockBalance";

export interface IBlendStockRequest {
    stockBalance: IStockBalance;
    lotNo: number;
    bagsToBlend: number;
    blendingDate: Date | string;
}
