import { DialogController } from "aurelia-dialog";
import { autoinject, observable } from "aurelia-framework";
import { connectTo } from "aurelia-store";
import { IStock } from "interfaces/stocks/IStock";
import _ from "lodash";
import { IState } from "store/state";
import { IStockListItem } from "./../../interfaces/stocks/IStockListItem";

@autoinject
@connectTo()
export class UncommittedStocksDialog {
  @observable public state: IState = undefined!;
  public list: IStock[] = [];

  constructor(
    private controller: DialogController
  ) { }

  protected stateChanged(state: IState) {
    this.list = _.cloneDeep(state.purchase.nonCommittedStocksList);
  }

  protected async activate(uncomittedStocks: IStock[]) {
    this.list = uncomittedStocks;
  }

  protected selectRow(item: IStockListItem) {
    item.selected = !item.selected;
  }

  protected okClicked() {
    this.controller.ok(this.getSelectedStocks());
  }

  private getSelectedStocks() {
    return this.list.filter(c => c.selected);
  }
}
