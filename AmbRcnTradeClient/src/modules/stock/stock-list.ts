import { IStockBalance } from "interfaces/stocks/IStockBalance";
import { isInRole } from "./../../services/role-service";
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
  private prmLocationId = "";

  public locations: IListItem[] = [];
  @observable selectedLocation: IListItem = undefined!
  protected lotNoList: string[] = [];

  protected filterLotNo: number | null = null;

  constructor(
    private stockService: StockService,
    private customerService: CustomerService,
    private router: Router
  ) { }

  protected stateChanged(state: IState) {
    this.list = state.stock.list;

    this.locations = _.cloneDeep(state.userFilteredCustomers);
    this.locations.unshift({ id: null, name: "[All]" });

    this.lotNoList = Array.from(new Set(this.list.map(item => `Lot no ${item.lotNo}`)));
    this.lotNoList.sort();
    this.lotNoList = this.lotNoList.filter(c => c);
  }

  protected async activate(prms: { lotNo?: number, locationId?: string }) {
    await this.customerService.loadCustomersForAppUserList();

    if (prms?.locationId) {
      this.prmLocationId = decodeParams(prms?.locationId);
    }
  }

  protected get stockBalance() {
    const filter = (list: IStockBalance[])=>list.filter(c=>c.lotNo===this.filterLotNo && c.locationId===this.selectedLocation.id);
    const bagsIn = this.list.filter(c => c.isStockIn && c.lotNo === this.filterLotNo && c.locationId===this.selectedLocation.id).reduce((a, b) => a += b.bagsIn, 0);
    const bagsOut = this.list.filter(c => !c.isStockIn).reduce((a, b) => a += b.bagsOut, 0);

    const weightIn = this.list.filter(c => c.isStockIn).reduce((a, b) => a += b.weightKgIn, 0);
    const weightOut = this.list.filter(c => !c.isStockIn).reduce((a, b) => a += b.weightKgOut, 0);

    return {
      bags: bagsIn - bagsOut,
      weightKg: weightIn - weightOut
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

  protected async runQuery() {
    this.stockService.loadStockList(this.selectedLocation.id);
  }

  protected encode(value: string) {
    return encodeParams(value);
  }

  protected get canNavigateToStock() {
    return isInRole(["admin", "user", "warehouseManager"], this.state);
  }

  protected get canNavigateToContainer() {
    return isInRole(["admin", "user", "warehouseManager"], this.state);
  }

  protected navigateToWarehouseList() {
    this.router.navigateToRoute("stockBalanceList");
  }
}
