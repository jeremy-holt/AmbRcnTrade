import { autoinject, bindable } from "aurelia-framework";
import { NavModel } from "aurelia-router";

@autoinject
export class SidenavDisplayRoutesCustomElement {
  @bindable public routes: NavModel[] = [];
}
