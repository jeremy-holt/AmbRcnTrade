import { IPurchaseListItem } from "interfaces/purchases/IPurchaseListItem";
import { IPaymentListItem } from "./IPaymentListItem";


export interface IPaymentDto {  
  paymentList: IPaymentListItem[];
  purchaseList: IPurchaseListItem[];
  purchaseValue: number;
  purchaseValueUsd: number;
  paymentValue: number;
  paymentValueUsd: number;
}
