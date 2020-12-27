export class ToJSONValueConverter {
  public toView(obj) {
    if (obj) {
      return JSON.stringify(obj, null, 2);
    }
  }
}
