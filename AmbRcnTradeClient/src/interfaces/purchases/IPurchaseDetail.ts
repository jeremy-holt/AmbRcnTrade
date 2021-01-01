import { Currency } from "constants/app-constants";
import { IStock } from "interfaces/stocks/IStock";

export interface IPurchaseDetail {
  stockIds?: string[];
  exchangeRate: number;
  currency: Currency;
  pricePerKg: number;
  priceAgreedDate: string;
  stocks: IStock[];

  values?: { bags: number; kor: number; count: number; moisture: number; };
}
