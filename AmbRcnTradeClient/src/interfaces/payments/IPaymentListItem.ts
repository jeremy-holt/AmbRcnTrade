import { Currency } from "constants/app-constants";


export interface IPaymentListItem {
  id: string;
  paymentDate: string | null;
  beneficiaryId: string;
  value: number;
  currency: Currency;
  exchangeRate: number;
  supplierId: string;
  beneficiaryName: string;
  supplierName: string;
  companyId: string;
}
