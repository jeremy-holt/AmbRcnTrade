import { Teu, TEU_LIST } from "constants/app-constants";
import { autoinject } from "aurelia-framework";

@autoinject
export class TeuFormatterValueConverter {
  public toView(teu: Teu) {
    return TEU_LIST.find(c => c.id === teu)?.name;
  }
}
