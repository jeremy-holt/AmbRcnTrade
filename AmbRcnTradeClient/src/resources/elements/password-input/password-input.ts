import { autoinject, bindable, bindingMode } from "aurelia-framework";

@autoinject
export class PasswordInput {
  @bindable({ defaultBindingMode: bindingMode.twoWay }) public value = "";
  @bindable public label = "";
}
