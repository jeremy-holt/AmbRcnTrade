import { autoinject, bindable, bindingMode } from "aurelia-framework";
import { trueFalse } from "./../../../core/helpers";
@autoinject
export class EmailInput {
  @bindable({ defaultBindingMode: bindingMode.twoWay }) public value: string = undefined!;
  @bindable public label: string = undefined!;
  @bindable public disabled = false;

  protected bind() {
    this.disabled = trueFalse(this.disabled) as boolean;
  }
}
