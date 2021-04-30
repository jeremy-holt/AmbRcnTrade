import { IListItem } from "core/interfaces/IEntity";
import { DialogController } from "aurelia-dialog";
import { autoinject, observable } from "aurelia-framework";
import { connectTo } from "aurelia-store";
import _ from "lodash";
import { IState } from "store/state";
import { IStockListItem } from "./../../interfaces/stocks/IStockListItem";

@autoinject
@connectTo()
export class UncommittedStocksDialog {
  @observable public state: IState = undefined!;
  public list: IStockListItem[] = [];
  public supplier: IListItem = undefined!;

  constructor(
    private controller: DialogController
  ) { }

  protected stateChanged(state: IState) {
    this.list = _.cloneDeep(state.purchase.nonCommittedStocksList);
  }

  protected async activate(model: { uncomittedStocks: IStockListItem[], supplier: IListItem }) {
    this.list = model.uncomittedStocks;
    this.supplier = model.supplier;
  }

  protected selectRow(item: IStockListItem) {
    item.selected = !item.selected;
  }

  protected okClicked() {
    this.controller.ok(this.getSelectedStocks());
  }

  private getSelectedStocks() {
    return this.list ? this.list.filter(c => c.selected) : [];
  }

  protected get canSave() {
    return this.getSelectedStocks().length > 0;
  }
}
