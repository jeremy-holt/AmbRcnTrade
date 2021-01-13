import { autoinject, bindable, bindingMode } from "aurelia-framework";
import { IListItem } from "../../../interfaces/IEntity";
import { trueFalse } from "./../../../core/helpers";
@autoinject
export class SelectInput {
  @bindable({ defaultBindingMode: bindingMode.twoWay }) public value: IListItem | string | null | undefined = undefined!;
  @bindable public source: IListItem[] = [];
  @bindable public label = "";
  @bindable public property = "name";
  @bindable public class = "";
  @bindable public disabled = false;
  @bindable public useModelId = false;
  @bindable public small = false;
  @bindable public matcher: unknown = undefined!;
  @bindable public warnNull = true;
  @bindable public required = "";

  private hasBound = false;

  constructor(private el: Element) { }

  protected bind() {
    this.disabled = trueFalse(this.disabled) as boolean;
    this.warnNull = trueFalse(this.warnNull) as boolean;
    this.hasBound = true;
  }

  protected valueChanged(value: never) {
    this.el.dispatchEvent(new CustomEvent("change", { bubbles: true, detail: value }));
  }

  protected get selectClass() {
    if (!this.hasBound) {
      return `${this.class} ${this.small ? "custom-select custom-select-sm" : "custom-select "}`;
    }
    const val = typeof this.value === "string" ? this.value : this.value?.id;
    return `${this.class} ${this.small ? "custom-select custom-select-sm" : "custom-select "} ${!val && this.warnNull ? "bg-mid-danger" : ""}`;
  }
}
