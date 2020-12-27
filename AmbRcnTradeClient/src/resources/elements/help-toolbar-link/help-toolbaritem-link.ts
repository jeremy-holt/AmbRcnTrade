import { autoinject } from "aurelia-framework";
import { Router } from "aurelia-router";

@autoinject
export class HelpToolbaritemLink {
  constructor(
    private router: Router
  ) { }

  protected gotoHelp() {
    this.router.navigateToRoute("workflow");
  }
}
