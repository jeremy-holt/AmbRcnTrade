import { autoinject, customAttribute } from "aurelia-framework";
import { trueFalse } from "../../core/helpers";

@autoinject
export class LinkDisabledCustomAttribute {
  public value: boolean = undefined!;
  constructor(
    private el: Element
  ) { }

  protected bind() {
    this.value = trueFalse(this.value) as boolean;

    if (this.el.localName === "a") {
      if (this.value) {
        this.el.classList.add("disabled");
      } else {
        this.el.classList.remove("disabled");
      }
    }
  }
}
