export class Guid {
  public static newGuid() {
    return "xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx".replace(/[xy]/g, c => {
      // tslint:disable-next-line:no-bitwise
      const r = Math.random() * 16 | 0;

      // tslint:disable-next-line:no-bitwise
      const v = c === "x" ? r : (r & 0x3 | 0x8);
      return v.toString(16);
    });
  }

  public static empty() {
    return "00000000-0000-0000-0000-000000000000";
  }
}
