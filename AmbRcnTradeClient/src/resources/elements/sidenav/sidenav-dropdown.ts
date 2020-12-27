import { randomHtmlId } from "./../../../core/helpers";
import { autoinject, bindable } from "aurelia-framework";
import { NavModel } from "aurelia-router";

@autoinject
export class SidenavDropdownCustomElement {
  @bindable public routes: NavModel[] = [];
  @bindable public title = "";
  public dropDownLink = "";

  protected bind() {
    this.dropDownLink = randomHtmlId(this.title);    
  }
}
