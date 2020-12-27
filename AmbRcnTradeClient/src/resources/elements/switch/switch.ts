import { autoinject, bindable, bindingMode } from "aurelia-framework";
import { randomInteger, trueFalse } from "../../../core/helpers";

@autoinject
export class Switch {
  @bindable({ defaultBindingMode: bindingMode.twoWay }) public checked: boolean = undefined!;
  @bindable public label = "";
  @bindable public class = "";
  @bindable public disabled = false;

  public localId = "";

  protected bind() {
    this.checked = trueFalse(this.checked) as boolean;
    this.disabled = trueFalse(this.disabled) as boolean;
    this.localId = randomInteger(10000, 100000).toString();
  }
}
