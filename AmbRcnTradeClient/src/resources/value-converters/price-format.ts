import { Currency, PriceUnit } from "./../../constants/app-constants";
import { IPrice } from "../../interfaces/contract-interfaces/IPrice";
import numbro from "numbro";

export class PriceFormatValueConverter {
  public toView(price: IPrice) {
    return `${this.getCurrencySymbol(price.currency)} ${this.getNumber(price.value)}${this.getPriceUnit(price.priceUnit)}`;
  }

  private getCurrencySymbol(currency: Currency) {
    switch (currency) {
      case Currency.USD:
        return "US$";
      case Currency.EUR:
        return "€";
      default:
        return currency;
    }
  }

  private getNumber(value: number) {
    return numbro(value).format("#,##0.00");
  }

  private getPriceUnit(unit: PriceUnit) {
    switch (unit) {
      case PriceUnit.PerLb:
        return "/lb";
      case PriceUnit.PerKg:
        return "/kg";
      case PriceUnit.PerMt:
        return "/mt";
    }
  }
}
