import { DialogService } from "aurelia-dialog";
import { autoinject, observable } from "aurelia-framework";
import { Router } from "aurelia-router";
import { connectTo } from "aurelia-store";
import _ from "lodash";
import { InspectionService } from "services/inspection-service";
import { IState } from "store/state";
import { Approval, CustomerGroup } from "./../../constants/app-constants";
import { encodeParams, getRavenRootId } from "./../../core/helpers";
import { ICustomerListItem } from "./../../interfaces/ICustomerListItem";
import { IInspectionListItem } from "./../../interfaces/inspections/IInspectionListItem";
import { IInspectionQueryParams } from "./../../interfaces/inspections/IInspectionQueryParams";
import { CustomerService } from "./../../services/customer-service";
import { StockNavigationDialog } from "./stock-navigation-dialog";

@autoinject
@connectTo()
export class InspectionList {
  @observable public state: IState;
  public list: IInspectionListItem[] = [];
  private customersList: ICustomerListItem[] = [];
  public warehouseList: ICustomerListItem[] = [];
  public suppliersList: ICustomerListItem[] = [];
  public buyersList: ICustomerListItem[] = [];
  @observable protected selectedWarehouse: ICustomerListItem = undefined!;
  @observable protected selectedSupplier: ICustomerListItem = undefined;
  @observable protected selectedBuyer: ICustomerListItem = undefined;
  public totals: { bags: number, weightKg: number, items: number, averagePrice: number, averageKor: number, averageMoisture: number, averageCount: number, bagsWithoutPrice: number, weightKgWithoutPrice: number, itemsWithoutPrice: number, totalWeightKg: number } = undefined!;

  constructor(
    private inspectionService: InspectionService,
    private router: Router,
    private dialogService: DialogService,
    private customerService: CustomerService
  ) {

  }

  protected async activate() {
    await this.customerService.loadCustomersForAppUserList();
  }

  protected stateChanged(state: IState) {
    this.list = _.cloneDeep(state.inspection.list);
    this.customersList = state.userFilteredCustomers;
    this.warehouseList = _.cloneDeep(this.customersList.filter(c => c.filter === CustomerGroup.Warehouse));
    this.suppliersList = _.cloneDeep(this.customersList.filter(c => c.filter === CustomerGroup.Supplier));
    this.buyersList = _.cloneDeep(this.customersList.filter(c => c.filter === CustomerGroup.Buyer));
    this.warehouseList.unshift({ id: null, name: "[All warehouses]" } as ICustomerListItem);
    this.suppliersList.unshift({ id: null, name: "[All suppliers]" } as ICustomerListItem);
    this.buyersList.unshift({ id: null, name: "[All buyers]" } as ICustomerListItem);

    this.list.forEach(c => {
      c.css = `text-right text-white ${c.approved === Approval.Approved ? "bg-success" : "bg-danger"}`;
    });
  }

  protected addInspection() {
    this.router.navigateToRoute("inspectionEdit");
  }

  protected encode(value: string) {
    return encodeParams(value);
  }

  protected openStockAllocationsList(item: IInspectionListItem) {
    this.dialogService.open(
      {
        viewModel: StockNavigationDialog,
        model: item
      });
  }

  protected navigateToStockList() {
    this.router.navigateToRoute("stockList");
  }

  protected getInspectionNumber(id: string) {
    return getRavenRootId(id);
  }

  protected async selectedWarehouseChanged() {
    await this.loadList();
  }

  protected async selectedSupplierChanged() {
    await this.loadList();
  }

  protected async selectedBuyerChanged() {
    await this.loadList();
  }

  protected async exportInspections() {
    const objectUrl = await this.inspectionService.exportInspections(this.list);
    window.location.href = objectUrl;
  }

  protected setTotals() {
    const list = this.list.filter(c => c.price > 0 && c.bags > 0);
    const bags = list.reduce((a, b) => a += b.bags, 0);
    const weightKg = list.reduce((a, b) => a += b.weightKg, 0);
    const items = list.length;

    const averagePrice = weightKg > 0 ? list.reduce((a, b) => a += (b.weightKg * b.price), 0) / weightKg : 0;
    const averageKor = weightKg > 0 ? list.reduce((a, b) => a += (b.weightKg * b.kor), 0) / weightKg : 0;
    const averageMoisture = weightKg > 0 ? list.reduce((a, b) => a += (b.weightKg * b.moisture), 0) / weightKg / 100 : 0;
    const averageCount = weightKg > 0 ? list.reduce((a, b) => a += (b.weightKg * b.count), 0) / weightKg : 0;

    const bagsWithoutPrice = this.list.filter(c => c.price === 0).reduce((a, b) => a += b.bags, 0);
    const weightKgWithoutPrice = this.list.filter(c => c.price === 0).reduce((a, b) => a += b.weightKg, 0);
    const itemsWithoutPrice = this.list.filter(c => c.price === 0).length;

    this.totals = { bags, weightKg, items, averagePrice, averageKor, averageMoisture, averageCount, bagsWithoutPrice, weightKgWithoutPrice, itemsWithoutPrice, totalWeightKg: weightKgWithoutPrice + weightKg };
  }

  private async loadList() {
    const prms: IInspectionQueryParams = {
      companyId: this.inspectionService.currentCompanyId(),
      warehouseId: this.selectedWarehouse?.id,
      approved: null,
      supplierId: this.selectedSupplier?.id,
      buyerId: this.selectedBuyer?.id
    };
    await this.inspectionService.loadList(prms);

    this.setTotals();
  }
}
