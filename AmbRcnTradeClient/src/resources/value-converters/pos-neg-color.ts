export class PosNegColorValueConverter {
  public toView(value: string): string {

    if (isNaN(+value)) {
      return "pos-number";
    }

    return parseFloat(value) < 0 ? "neg-number" : "pos-number";
  }
}
