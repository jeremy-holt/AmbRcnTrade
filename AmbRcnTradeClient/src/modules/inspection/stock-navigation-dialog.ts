import { DialogController } from "aurelia-dialog";
import { autoinject } from "aurelia-framework";
import { Router } from "aurelia-router";
import { IInspectionListItem } from "./../../interfaces/inspections/IInspectionListItem";

@autoinject
export class StockNavigationDialog {
  public model: IInspectionListItem;

  constructor(
    private controller: DialogController,
    private router: Router
  ) { }

  protected activate(model: IInspectionListItem) {
    this.model = model;
  }

  protected navigateToStockReference(index: number) {
    this.controller.cancel();
    this.router.navigateToRoute("stockBalanceList", { lotNo: this.model.stockReferences[index].lotNo });
  }
}
