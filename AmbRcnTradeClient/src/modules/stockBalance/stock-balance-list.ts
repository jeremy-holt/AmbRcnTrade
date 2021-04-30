import { ICustomerListItem } from "core/interfaces/ICustomerListItem";
import { DialogService } from "aurelia-dialog";
import { autoinject, observable } from "aurelia-framework";
import { Router } from "aurelia-router";
import { connectTo } from "aurelia-store";
import { encodeParams } from "core/helpers";
import _ from "lodash";
import { StockManagementService } from "services/stock-management-service";
import { IState } from "store/state";
import { ContainerStatus, CustomerGroup, IStockBalanceFilterItem, StockBalanceFilter, STOCK_BALANCE_FILTER_LIST } from "./../../constants/app-constants";
import { IAvailableContainer } from "./../../interfaces/stockManagement/IAvailableContainerItem";
import { IStockBalance } from "./../../interfaces/stocks/IStockBalance";
import { isInRole } from "core/services/role-service";
import { StockService } from "./../../services/stock-service";
import { StuffContainerDialog } from "./stuff-container-dialog";
import { CustomerService } from "core/services/customer-service";
import { StockBalanceFilterValueConverter } from "./stock-balance-filter-valueConverter";

@autoinject
@connectTo()
export class StockBalanceList {
  @observable public state: IState = undefined!;
  protected list: IStockBalance[] = [];
  protected availableContainersList: IAvailableContainer[] = [];
  protected numberEmptyContainers = 0;
  private currentLotNo: number = undefined!;

  protected warehouses: ICustomerListItem[] = [];
  protected selectedWarehouse: ICustomerListItem = undefined!;

  public stocksFilter = STOCK_BALANCE_FILTER_LIST;
  protected selectedStocksFilter: IStockBalanceFilterItem = this.stocksFilter.find(c => c.id === StockBalanceFilter.WithStockBalance);

  constructor(
    private stockService: StockService,
    private dialogService: DialogService,
    private stockManagementService: StockManagementService,
    private customerService: CustomerService,
    private router: Router,
    private stockBalanceFilterValueConverter: StockBalanceFilterValueConverter
  ) { }

  public async activate(prms: { lotNo: number, locationId: string }) {
    await this.stockManagementService.getAvailableContainers();
    await this.stockService.loadStockBalanceList(prms?.locationId);
    await this.customerService.loadCustomersForAppUserList();
    this.currentLotNo = +prms?.lotNo;
  }


  protected stateChanged(state: IState) {
    this.list = _.cloneDeep(state.stock.stockBalanceList);
    this.list.forEach(c => c.selected = c.lotNo === this.currentLotNo);
    this.availableContainersList = _.cloneDeep(state.stockManagement.availableContainers);
    this.numberEmptyContainers = this.availableContainersList.filter(c => c.status === ContainerStatus.Empty).length;

    this.warehouses = _.cloneDeep(state.userFilteredCustomers).filter(c => c.filter === CustomerGroup.Warehouse);
    this.warehouses.unshift({ id: null, name: "[All]" } as ICustomerListItem);
  }

  protected encode(value: string) {
    return encodeParams(value);
  }

  protected async openStuffContainerDialog(stockBalance: IStockBalance) {
    await this.dialogService.open(
      {
        viewModel: StuffContainerDialog,
        model: { stockBalance }
      }
    ).whenClosed(async result => {
      if (!result.wasCancelled) {
        const { containerId, bags, weightKg, status, stuffingDate } = result.output as { containerId: string; bags: number; weightKg: number; status: ContainerStatus, stuffingDate: string };
        await this.stockManagementService.stuffContainer(containerId, stuffingDate, stockBalance, bags, weightKg, status);
        await this.stockService.loadStockBalanceList(null);
      }
    });
  }

  protected get canAddContainer() {
    return isInRole(["admin", "user", "warehouseManager"], this.state);
  }

  protected navigateToContainerList() {
    this.router.navigateToRoute("containerList");
  }

  protected get summary() {
    return {
      bags: this.list.reduce((a, b) => a += b.balance, 0),
      weightKg: this.list.reduce((a, b) => a += b.balanceWeightKg, 0)
    };
  }

  protected goToStockList(item: IStockBalance) {
    this.currentLotNo = item.lotNo;
    this.router.navigateToRoute("stockBalanceList", { lotNo: item.lotNo }, { trigger: false, replace: true });
    this.router.navigateToRoute("stockList", { lotNo: item.lotNo });
  }

  protected get totals() {
    const list = this.stockBalanceFilterValueConverter.toView(this.list, this.selectedStocksFilter, this.selectedWarehouse);
    
    const bags = list.reduce((a, b) => a += b.balance, 0);
    const weightKg = list.reduce((a, b) => a += b.balanceWeightKg, 0);
    return { bags, weightKg };
  }
}
