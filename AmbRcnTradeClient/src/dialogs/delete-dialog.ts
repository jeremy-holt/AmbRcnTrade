import { DialogController } from "aurelia-dialog";
import { autoinject, bindable } from "aurelia-framework";

@autoinject
export class DeleteDialog {
  @bindable public body = "Are you sure you wish to delete this?";

  constructor(
    public controller: DialogController
  ) { }

  protected activate(model: { body: string }) {
    if (model?.body) {
      this.body = model.body;
    }
  }
}
