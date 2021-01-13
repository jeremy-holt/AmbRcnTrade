import { autoinject, bindable, bindingMode } from "aurelia-framework";
import { trueFalse } from "./../../../core/helpers";

@autoinject
export class NumberInputSuffix {
  @bindable({ defaultBindingMode: bindingMode.twoWay }) public value = "";
  @bindable public label = "";
  @bindable public disabled = false;
  @bindable public class = "";
  @bindable public suffix = "";
  @bindable public prefix = "";
  @bindable public min = 0;
  @bindable public max: number = undefined!;
  @bindable public step: number = undefined!;
  @bindable public small=false;
  @bindable public required="";

  constructor(private el:Element){}

  protected bind() {
    this.disabled = trueFalse(this.disabled) as boolean;
  }

  protected valueChanged(value: string){
    this.el.dispatchEvent(new CustomEvent("change",{bubbles:true, detail:+value}));
  }

  protected get formControlClass(){
    return `${this.class} ${this.small ? "form-control form-control-sm" : "form-control"}`;
  }
}
