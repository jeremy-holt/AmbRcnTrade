import { autoinject } from "aurelia-framework";
import { Currency } from "constants/app-constants";
import numbro from "numbro";

@autoinject
export class CurrencyFormatValueConverter {
  public toView(value: number, currency: Currency) {

    const format: numbro.Format = currency === Currency.CFA
      ? { thousandSeparated: true, mantissa: 0 }
      : { thousandSeparated: true, mantissa: 2 };

    if (value === 0) {
      return "";
    }

    return `${currency.toUpperCase()} ${numbro(value).format(format)}`;
  }
}
