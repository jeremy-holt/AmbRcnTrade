import { autoinject, observable } from "aurelia-framework";
import { Router } from "aurelia-router";
import { connectTo } from "aurelia-store";
import { StockBalanceFilter, STOCK_BALANCE_FILTER_LIST } from "constants/app-constants";
import { IListItem } from "core/interfaces/IEntity";
import _ from "lodash";
import { IState } from "store/state";
import "../../core/helpers";
import { decodeParams, distinctBy, encodeParams, getRavenRootId } from "./../../core/helpers";
import { IStockListItem } from "./../../interfaces/stocks/IStockListItem";
import { CustomerService } from "../../core/services/customer-service";
import { isInRole } from "core/services/role-service";
import { StockService } from "./../../services/stock-service";


@autoinject
@connectTo()
export class StockList {
  @observable state: IState = undefined!;
  public list: IStockListItem[] = [];
  private prmLocationId = "";
  public stocksFilter = STOCK_BALANCE_FILTER_LIST;
  protected selectedStocksFilter = this.stocksFilter.find(c => c.id === StockBalanceFilter.WithStockBalance);
  
  public locations: IListItem[] = [];
  @observable selectedLocation: IListItem = undefined!
  protected lotNoList: { id: string | number, name: string }[] = [];

  protected filterLotNo: number | null = null;

  constructor(
    private stockService: StockService,
    private customerService: CustomerService,
    private router: Router
  ) { }

  protected stateChanged(state: IState) {
    this.list = _.cloneDeep( state.stock.list);
    this.list.forEach(c=>c.selected= this.filterLotNo === c.lotNo);

    this.locations = _.cloneDeep(state.userFilteredCustomers);
    this.locations.unshift({ id: null, name: "[All]" });

    this.lotNoList = this.list.map(item => ({ id: item.lotNo, name: `Lot no ${item.lotNo}` }));
    this.lotNoList = distinctBy(this.lotNoList, "id");

    this.lotNoList.sort((a, b) => a.id > b.id ? 1 : 0);
    this.lotNoList.unshift({ id: null, name: "[All]" });
  }

  protected async activate(prms: { lotNo?: number, locationId?: string }) {
    await this.customerService.loadCustomersForAppUserList();

    if (prms?.locationId) {
      this.prmLocationId = decodeParams(prms?.locationId);
    }
    this.filterLotNo = +prms?.lotNo;    
  }

  protected get stockBalance() {
    const filterStockIn = (item: IStockListItem, isStockIn: boolean) => item.isStockIn === isStockIn;
    const filterLocation = (item: IStockListItem) => item.locationId === this.selectedLocation?.id;
    const filterLotNumber = (item: IStockListItem) => item.lotNo === this.filterLotNo;

    const reduce = (list: IStockListItem[], key: keyof IStockListItem, isStockIn: boolean) => {
      return list.filter(c => filterStockIn(c, isStockIn)).reduce((a: number, item: IStockListItem) => a += (item[key] as number), 0);
    };

    const filter = (item: IStockListItem) => {
      if (this.selectedLocation?.id && this.filterLotNo !== null) {
        return filterLocation(item) && filterLotNumber(item);
      } else if (this.selectedLocation?.id) {
        return filterLocation(item);
      } else if (this.filterLotNo !== null) {
        return filterLotNumber(item);
      }
      else {
        return true;
      }
    };

    const WEIGHT_IN = true;
    const WEIGHT_OUT = false;

    const bagsIn = reduce(this.list.filter(filter), "bagsIn", WEIGHT_IN);
    const bagsOut = reduce(this.list.filter(filter), "bagsOut", WEIGHT_OUT);
    const weightIn = reduce(this.list.filter(filter), "weightKgIn", WEIGHT_IN);
    const weightOut = reduce(this.list.filter(filter), "weightKgOut", WEIGHT_OUT);

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

  protected getInspectionNumber(id: string){
    return getRavenRootId(id);
  }
}
