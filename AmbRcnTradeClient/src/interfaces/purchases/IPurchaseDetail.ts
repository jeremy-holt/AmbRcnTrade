import { Currency } from "constants/app-constants";
import { IStock } from "interfaces/stocks/IStock";

export interface IPurchaseDetail {
    stockIds: string[];
    exchangeRate: number;
    currency: Currency;
    pricePerKg: number;
    date: Date | string;
    stocks: IStock[];
}
