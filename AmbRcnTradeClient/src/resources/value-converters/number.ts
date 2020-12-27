import { isNumber } from "util";

export class NumberValueConverter {
  public fromView(val: string) {
    return (val === null || val === undefined || val === "" || Number.isNaN(Number(val))) ? 0 : Number(val);
  }
}
