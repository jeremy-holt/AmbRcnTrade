import { Currency } from "constants/app-constants";

export interface IPayment {
  paymentDate: string | null;
  beneficiaryId: string;
  value: number;
  currency: Currency;
  exchangeRate: number;
  supplierId: string;
  notes: string;
  id: string;
  name: string;
  companyId: string;
  paymentNo: number;
}

