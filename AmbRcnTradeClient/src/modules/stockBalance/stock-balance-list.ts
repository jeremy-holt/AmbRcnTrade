import { StockService } from "./../../services/stock-service";
import { IStockBalanceListItem } from "./../../interfaces/stocks/IStockBalanceListItem";
import { autoinject, observable } from "aurelia-framework";
import { connectTo } from "aurelia-store";
import { IState } from "store/state";
import _ from "lodash";

@autoinject
@connectTo()
export class StockBalanceList {
  @observable public state: IState = undefined!;
  protected list: IStockBalanceListItem[] = [];

  constructor(
    private stockService: StockService
  ) { }

  public async activate(prms: { lotNo: number, locationId: string }) {
    await this.stockService.loadStockBalanceList(prms?.lotNo, prms?.locationId);
  }

  protected stateChanged(state: IState) {
    this.list = _.cloneDeep(state.stock.stockBalanceList);
  }
}
