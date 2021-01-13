import { autoinject, bindable, bindingMode } from "aurelia-framework";
import { trueFalse } from "./../../../core/helpers";

@autoinject
export class TextInput {
  @bindable({ defaultBindingMode: bindingMode.twoWay }) public value: string = undefined!;
  @bindable public label: string = undefined!;
  @bindable public disabled = false;
  @bindable public small = false;
  @bindable public placeholder: string = undefined!;
  @bindable public maxlength = 1000;
  @bindable public required = "";

  protected numChars = 0;

  constructor(
    private el: Element
  ) { }

  protected bind() {
    this.disabled = trueFalse(this.disabled) as boolean;
    this.placeholder = this.placeholder ?? this.label;
    this.numChars = this.value?.length;
  }

  protected valueChanged() {
    this.numChars = this.value?.length;
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

  protected get showMaxLength() {
    return this.maxlength !== 1000 && this.numChars > 0;
  }

  protected get showRequired() {
    return this.required?.length > 0 && !this.value;
  }
}
