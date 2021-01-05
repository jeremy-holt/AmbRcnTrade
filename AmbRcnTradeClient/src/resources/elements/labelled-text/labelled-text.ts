import { bindable } from "aurelia-framework";

export class LabelledText {
  @bindable public label="";
  @bindable public value="";
  @bindable public class="";
  @bindable public float="";
}
