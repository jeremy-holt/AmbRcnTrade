import { autoinject, bindable, bindingMode, observable } from "aurelia-framework";
import { trueFalse } from "./../../../core/helpers";

@autoinject
export class DateInput {
  @bindable({ defaultBindingMode: bindingMode.twoWay }) public value: string | null = undefined!;
  @bindable public label: string = undefined!;
  @bindable({ defaultBindingMode: bindingMode.twoWay }) public disabled = false;
  @bindable small = false;
  @bindable fitcontent = false;

  @observable protected selectedValue: string | null = undefined!;

  protected bind() {
    this.disabled = trueFalse(this.disabled) as boolean;
    this.selectedValue = this.value;
  }

  protected selectedValueChanged(date: string) {
    this.value = date === "" ? null : date;
  }

  protected get formControlClass() {    
    return this.small ? "form-control form-control-sm" : "form-control";
  }

  protected get textColor(){
    return this.value ? "":"silver";
  }

  protected get fitContentStyle() {
    return `color: ${this.textColor}; max-width: ${this.fitcontent ? "fit-content":""}`;    
  }
}
