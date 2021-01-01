import { Currency } from "constants/app-constants";
import { IStockListItem } from "./../stocks/IStockListItem";

export interface IPurchaseDetail {
  stockIds?: string[];
  exchangeRate: number;
  currency: Currency;
  pricePerKg: number;
  priceAgreedDate: string;
  stocks: IStockListItem[];

  values?: { bags: number; kor: number; count: number; moisture: number; };
}
