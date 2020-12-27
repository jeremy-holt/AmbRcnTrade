import { EventAggregator } from "aurelia-event-aggregator";
import { autoinject, bindable, bindingMode } from "aurelia-framework";
import { Router } from "aurelia-router";
import { connectTo } from "aurelia-store";
import { trueFalse } from "./../../../core/helpers";

@autoinject
@connectTo()
export class Toolbar {
  @bindable public caption: string = undefined!;
  @bindable public listRoute: string = undefined!;
  @bindable public showLogin = true;
  @bindable public save: () => any = undefined!;
  @bindable({ defaultBindingMode: bindingMode.twoWay }) public disabledSave = false;

  constructor(
    private ea: EventAggregator,
    private router: Router
  ) {

  }

  public get showSaveButton() {
    return this.save !== undefined;
  }

  protected callSave() {
    if (!this.disabledSave) {
      this.save();
    }
  }

  protected openSideBar() {
    this.ea.publish("openSideNav");
  }

  protected navigateBack() {
    this.router.navigateBack();
  }

  protected bind() {
    this.showLogin = trueFalse(this.showLogin) as boolean;
  }
}
