import { autoinject, bindable, bindingMode, observable } from "aurelia-framework";
import { Currency, CURRENCIES_LIST } from "../../constants/app-constants";
import { ICurrency } from "../../core/interfaces/ICurrency";


@autoinject
export class CurrenciesListCustomElement {
  @bindable({ defaultBindingMode: bindingMode.twoWay }) public value: Currency = undefined!;
  @bindable public disabled = "false";
  @bindable public label: string = undefined!;
  @bindable public class: string = undefined!;
  @observable public selectedValue: ICurrency = undefined!;
  public currenciesList: ICurrency[] = CURRENCIES_LIST;

  protected bind() {
    this.selectedValue = this.currenciesList.find(c => c.id === this.value) ?? this.currenciesList[0];
  }

  protected selectedValueChanged(value: ICurrency) {
    this.value = value.id;
  }

  protected matcher = (a: ICurrency, b: ICurrency) => {
    return a.id === b?.id;
  }
}
