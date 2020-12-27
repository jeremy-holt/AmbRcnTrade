import { autoinject, bindable } from "aurelia-framework";
import { trueFalse } from "../../../core/helpers";

@autoinject
export class ToolbarItem {
  @bindable public icon: string = undefined!;
  @bindable public click: void = undefined!;
  @bindable public disabled = false;
  @bindable public label = "";

  protected bind() {
    this.disabled = trueFalse(this.disabled) as boolean;
  }
}
