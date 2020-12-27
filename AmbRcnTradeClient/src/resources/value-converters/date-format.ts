import moment from "moment";
import "moment/locale/fr";
import "moment/locale/pt";
import "moment/locale/vi";


export class DateFormatValueConverter {
  public toView(value: string, format: string) {
    if (!value) {
      return value;
    }

    if (!isNaN(+value)) {
      return value;
    }

    if (value && value.startsWith("Q") || value.toLowerCase().startsWith("total") || value.toLowerCase().startsWith("avg") ||
      value.toLowerCase().startsWith("average") || value.toLocaleLowerCase().startsWith("year")) {
      return value;
    }

    if (moment(value).isValid()) {
      return moment(value).format(format || "DD/MM/YYYY");
    }
    return value;
  }

  public fromView(value) {
    return new Date(value);
  }
}
