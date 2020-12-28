import { autoinject, bindable, bindingMode } from "aurelia-framework";
import countries from "../../core/countries-list.json";
import { trueFalse } from "../../core/helpers";

@autoinject
export class CountriesListCustomElement {
  public countriesList = countries;

  @bindable({ defaultBindingMode: bindingMode.twoWay }) public value: string = undefined!;
  @bindable public label = "Country";
  @bindable public disabled = false;

  protected bind() {
    this.disabled = trueFalse(this.disabled) as boolean;
  }
}
