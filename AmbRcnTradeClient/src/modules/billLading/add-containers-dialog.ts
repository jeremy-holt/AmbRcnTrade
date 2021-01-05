import { DialogController } from "aurelia-dialog";
import { autoinject } from "aurelia-framework";
import { INotLoadedContainer } from "./../../interfaces/shipping/INotLoadedContainer";

@autoinject
export class AddContainersDialog {
  protected list: INotLoadedContainer[] = [];

  constructor(
    protected controller: DialogController
  ) { }

  protected activate(model: { unloadedContainers: INotLoadedContainer[] }) {
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
