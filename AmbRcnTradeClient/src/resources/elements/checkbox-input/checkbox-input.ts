import { autoinject, bindable, bindingMode, observable } from "aurelia-framework";
import { randomInteger, trueFalse } from "../../../core/helpers";

@autoinject
export class CheckboxInput {
  @bindable({ defaultBindingMode: bindingMode.twoWay }) public value = false;
  @bindable public disabled = false;
  @bindable public label: string = undefined!;

  @observable public selectedValue = false;
  public checkboxId: string = undefined!;

  protected selectedValueChanged(value: boolean) {
    this.value = value;
  }

  protected bind() {
    this.checkboxId = `checkbox_${randomInteger(0, 50000)}`;
    this.disabled = trueFalse(this.disabled) as boolean;
    this.selectedValue = this.value;
  }
}
