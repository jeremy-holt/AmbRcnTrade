import { IPurchaseListItem } from "interfaces/purchases/IPurchaseListItem";
import { IPaymentListItem } from "./IPaymentListItem";
import { IPayment } from "./IPayment";


export interface IPaymentDto {
  payment: IPayment;
  paymentList: IPaymentListItem[];
  purchaseList: IPurchaseListItem[];
}
