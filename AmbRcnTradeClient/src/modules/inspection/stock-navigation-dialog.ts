import { Router } from "aurelia-router";
import { IInspectionListItem } from "./../../interfaces/inspections/IInspectionListItem";
import { DialogController } from "aurelia-dialog";
import { autoinject } from "aurelia-framework";

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
    this.router.navigateToRoute("stockEdit", { id: this.model.stockReferences[index].stockId });    
  }
}
