import numbro from "numbro";
import { log } from "../../core/log";
import { LanguageService } from "../../services/language-service";

export class NumberFormatValueConverter {


  public toView(value: any, format: string) {
    if (value && isNaN(value)) {
      value = null;
    }

    if (value === null || value === undefined) {
      return "";
    }

    numbro.setLanguage(LanguageService.cultureFull);

    try {
      if (format) {
        const arr = format.split("|");
        format = arr[0];

        if (arr.length > 1 && (value === 0 || value === null || value === undefined)) {
          return arr[1].trim() === "null" ? "" : arr[1].trim();
        }

        if (format.endsWith("%")) {
          return numbro(value).format(format);
        }

        if (format.trim() === "0,0" || format.trim() === "0") {
          return numbro(value).format({ thousandSeparated: true, mantissa: 0 });
        }

      }

      if (format === "0,0" || format === "0") {
        return numbro(value).format({ thousandSeparated: true, mantissa: 0 });
      }


      return numbro(value || 0).format(format || "0,0");
    } catch (e) {
      log.error("Error in numberFormat. Value is", format, value);
      throw e;
    }
  }
}

