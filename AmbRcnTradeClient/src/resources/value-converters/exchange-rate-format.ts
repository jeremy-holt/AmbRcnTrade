import numbro from "numbro";
import { autoinject } from "aurelia-framework";

@autoinject
export class ExchangeRateFormatValueConverter {
  public toView(exchangeRate: string) {
    if (!exchangeRate) {
      return "";
    }

    const rate = parseFloat(exchangeRate);

    if (rate === 1) {
      return "";
    }

    if (rate >= 10) {
      return numbro(rate).format("0.00");
    }
    
    if (rate <= 0.01) {
      return `(${numbro(1 / rate).format("0.00")})`;
    }
    
    return numbro(rate).format("0.0000");
  }
}
