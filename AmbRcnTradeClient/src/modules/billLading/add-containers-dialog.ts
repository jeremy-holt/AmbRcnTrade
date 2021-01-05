import { IContainer } from "./../../interfaces/shipping/IContainer";
import { DialogController } from "aurelia-dialog";
import { autoinject } from "aurelia-framework";

@autoinject
export class AddContainersDialog {
  protected list: IContainer[] = [];

  constructor(
    protected controller: DialogController
  ) { }

  protected activate(model: { unloadedContainers: IContainer[] }) {
    this.list = model.unloadedContainers;
  }

  protected selectRow(index: number) {
    this.list[index].selected = !this.list[index].selected;
  }

  protected cancelClicked() {
    this.list.forEach(c => c.selected = false);
    this.controller.cancel();
  }

  protected okClicked() {
    this.controller.ok(this.selectedRows);
  }

  private get selectedRows() {
    return this.list.filter(c => c.selected);
  }
}
