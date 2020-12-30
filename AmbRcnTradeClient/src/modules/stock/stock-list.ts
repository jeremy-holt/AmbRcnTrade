import { encodeParams } from "./../../core/helpers";
import { CustomerService } from "./../../services/customer-service";
import { IListItem } from "interfaces/IEntity";
import { IStockListItem } from "./../../interfaces/stocks/IStockListItem";
import { IState } from "store/state";
import { Router } from "aurelia-router";
import { StockService } from "./../../services/stock-service";
import { autoinject, observable } from "aurelia-framework";
import { connectTo } from "aurelia-store";
import _ from "lodash";

@autoinject
@connectTo()
export class StockList {
  @observable state: IState = undefined!;
  public list: IStockListItem[] = [];
  public selectedLotNo: number = undefined!;

  public locations: IListItem[] = [];
  @observable selectedLocation: IListItem = undefined!

  constructor(
    private stockService: StockService,
    private customerService: CustomerService,
    private router: Router
  ) { }

  protected stateChanged(state: IState) {
    this.list = state.stock.list;

    this.locations = _.cloneDeep(state.userFilteredCustomers);
    this.locations.unshift({ id: null, name: "[All]" });
  }

  protected async activate() {
    await this.customerService.loadCustomersForAppUserList();
  }

  protected async selectedLocationChanged() {
    await this.runQuery();
  }

  protected addStock() {
    this.router.navigateToRoute("stockEdit", { id: null });
  }

  protected async runQuery() {
    this.stockService.loadStockList(this.selectedLotNo, this.selectedLocation.id);
  }

  protected encode(value: string){
    return encodeParams(value);
  }
}
