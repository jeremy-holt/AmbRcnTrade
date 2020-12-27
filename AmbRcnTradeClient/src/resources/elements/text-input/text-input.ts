import { autoinject, bindable, bindingMode } from "aurelia-framework";
import { trueFalse } from "./../../../core/helpers";

@autoinject
export class TextInput {
  @bindable({ defaultBindingMode: bindingMode.twoWay }) public value: string = undefined!;
  @bindable public label: string = undefined!;
  @bindable public disabled = false;
  @bindable public small = false;
  @bindable public placeholder: string = undefined!;

  constructor(
    private el: Element
  ) { }

  protected bind() {
    this.disabled = trueFalse(this.disabled) as boolean;
    this.placeholder = this.placeholder ?? this.label;
  }

  protected valueChanged() {
    this.notify();
  }

  protected notify() {
    this.el.dispatchEvent(new CustomEvent("change", {
      bubbles: true,
      detail: this.value
    }));
  }

  protected get formControlClass() {
    return this.small ? "form-control form-control-sm" : "form-control";
  }
}
