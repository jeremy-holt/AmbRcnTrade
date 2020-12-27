import { autoinject, bindable, bindingMode } from "aurelia-framework";
import { trueFalse } from "./../../../core/helpers";

@autoinject
export class TextAreaInput {
  @bindable({ defaultBindingMode: bindingMode.twoWay }) public value: string = undefined!;
  @bindable public label: string = undefined!;
  @bindable public class: string = undefined!;
  @bindable public rows = 6;
  @bindable public disabled = false;
  @bindable public small = false;

  protected bind() {
    this.disabled = trueFalse(this.disabled) as boolean;
  }

  protected get formControlClass() {
    return `${this.class} ${this.small ? "form-control form-control-sm" : "form-control"}`;
  }
}
