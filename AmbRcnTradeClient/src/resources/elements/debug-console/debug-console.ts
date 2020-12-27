import { autoinject, bindable } from "aurelia-framework";
import { trueFalse } from "../../../core/helpers";
import { log } from "../../../core/log";

@autoinject
export class DebugConsole {
  @bindable public model: any;
  public show = false;

  protected debug() {
    log.info("TCL: -------------------------------------------------");
    log.info("TCL: DebugConsole -> debug -> model", this.model);
    log.info("TCL: -------------------------------------------------");
  }

  protected bind() {
    this.show = trueFalse(this.show) as boolean;
  }
}
