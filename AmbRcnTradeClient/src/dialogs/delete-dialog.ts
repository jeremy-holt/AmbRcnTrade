import { DialogController } from "aurelia-dialog";
import { autoinject, bindable } from "aurelia-framework";

@autoinject
export class DeleteDialog {
  @bindable public body = "Are you sure you wish to delete this?";
  @bindable public header = "Delete";

  constructor(
    public controller: DialogController
  ) { }

  protected activate(model: { header: string, body: string }) {
    if (model?.body) {
      this.body = model.body;
    }
    if (model?.header) {
      this.header = model.header;
    }
  }
}
