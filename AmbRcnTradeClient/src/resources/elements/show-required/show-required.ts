import { bindable } from "aurelia-framework";

export class ShowRequired {
  @bindable public value = undefined!;
  @bindable public required = "";

  protected get showRequired() {
    return this.required?.length > 0 && !this.value;
  }
}
