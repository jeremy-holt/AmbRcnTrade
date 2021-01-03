import { autoinject, observable } from "aurelia-framework";
import { Router } from "aurelia-router";
import { connectTo } from "aurelia-store";
import { IListItem } from "interfaces/IEntity";
import _ from "lodash";
import { IState } from "store/state";
import { decodeParams, encodeParams } from "./../../core/helpers";
import { IStockListItem } from "./../../interfaces/stocks/IStockListItem";
import { CustomerService } from "./../../services/customer-service";
import { StockService } from "./../../services/stock-service";

@autoinject
@connectTo()
export class StockList {
  @observable state: IState = undefined!;
  public list: IStockListItem[] = [];
  public selectedLotNo: number = undefined!;
  private prmLocationId = "";

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

  protected async activate(prms: { lotNo?: number, locationId?: string }) {
    await this.customerService.loadCustomersForAppUserList();
    this.selectedLotNo = +prms?.lotNo === -1 ? null : prms?.lotNo;

    if (prms?.locationId) {
      this.prmLocationId = decodeParams(prms?.locationId);
    }
  }

  protected get stockBalance() {
    return {
      bags: this.list.reduce((a, b) => a += (b.bagsIn - b.bagsOut), 0),
      weightKg: this.list.reduce((a, b) => a += (b.weightKgIn - b.weightKgOut), 0)
    };
  }

  protected bind() {
    if (this.prmLocationId) {
      this.selectedLocation = this.locations.find(c => c.id === decodeParams(this.prmLocationId));
    }
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

  protected encode(value: string) {
    return encodeParams(value);
  }
}
