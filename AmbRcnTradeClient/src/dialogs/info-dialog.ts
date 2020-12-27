import { DialogController } from "aurelia-dialog";
import { autoinject } from "aurelia-framework";

@autoinject
export class InfoDialog {
  public model: { header: string; body: string } = undefined!;

  constructor(
    private controller: DialogController
  ) { }

  public activate(model: { header: string; body: string }) {
    this.model = model;
  }
}
