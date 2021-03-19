import { ICustomerListItem } from "./../../interfaces/ICustomerListItem";
import { IInspectionQueryParams } from "./../../interfaces/inspections/IInspectionQueryParams";
import { DialogService } from "aurelia-dialog";
import { autoinject, observable } from "aurelia-framework";
import { Router } from "aurelia-router";
import { connectTo } from "aurelia-store";
import _ from "lodash";
import { InspectionService } from "services/inspection-service";
import { IState } from "store/state";
import { Approval, APPROVAL_LIST, CustomerGroup } from "./../../constants/app-constants";
import { encodeParams, getRavenRootId } from "./../../core/helpers";
import { IInspectionListItem } from "./../../interfaces/inspections/IInspectionListItem";
import { CustomerService } from "./../../services/customer-service";
import { StockNavigationDialog } from "./stock-navigation-dialog";

@autoinject
@connectTo()
export class InspectionList {
  @observable public state: IState;
  public list: IInspectionListItem[] = [];
  public approvalList = _.cloneDeep(APPROVAL_LIST);
  @observable selectedApproval = undefined!;
  private customersList: ICustomerListItem[] = [];
  public warehouseList: ICustomerListItem[] = [];
  public suppliersList: ICustomerListItem[] = [];
  @observable protected selectedWarehouse: ICustomerListItem = undefined!;
  @observable protected selectedSupplier: ICustomerListItem = undefined;
  public totals: { bags: number, weightKg: number, items: number, averagePrice: number, averageKor: number, averageMoisture: number, averageCount: number } = undefined!;

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
    this.warehouseList.unshift({ id: null, name: "[All warehouses]" } as ICustomerListItem);
    this.suppliersList.unshift({ id: null, name: "[All suppliers]" } as ICustomerListItem);

    this.list.forEach(c => {
      c.css = `text-right text-white ${c.approved === Approval.Approved ? "bg-success" : "bg-danger"}`;
    });
  }

  protected bind() {
    this.approvalList.unshift({ id: null, name: "[All]" });
    this.selectedApproval = this.approvalList[0];
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

  protected async selectedApprovalChanged() {
    await this.loadList();
  }

  protected async exportInspections() {
    const objectUrl = await this.inspectionService.exportInspections(this.list);
    window.location.href = objectUrl;
  }

  protected setTotals() {
    const bags = this.list.reduce((a, b) => a += b.bags, 0);
    const weightKg = this.list.reduce((a, b) => a += b.weightKg, 0);
    const items = this.list.length;

    const listWithNonZeroPrice = this.list.filter(c=>c.price>0);
    const weightWithPriceKg = listWithNonZeroPrice.reduce((a, b) => a += b.weightKg, 0);
    const averagePrice = weightKg > 0 ? listWithNonZeroPrice.reduce((a, b) => a += (b.weightKg * b.price), 0) / weightWithPriceKg : 0;
    const averageKor = weightKg > 0 ? this.list.reduce((a, b) => a += (b.weightKg * b.kor), 0) / weightKg : 0;
    const averageMoisture = weightKg > 0 ? this.list.reduce((a, b) => a += (b.weightKg * b.moisture), 0) / weightKg / 100 : 0;
    const averageCount = weightKg > 0 ? this.list.reduce((a, b) => a += (b.weightKg * b.count), 0) / weightKg : 0;

    this.totals = { bags, weightKg, items, averagePrice, averageKor, averageMoisture, averageCount };
  }

  private async loadList() {
    const prms: IInspectionQueryParams = {
      companyId: this.inspectionService.currentCompanyId(),
      warehouseId: this.selectedWarehouse?.id,
      approved: this.selectedApproval.id,
      supplierId: this.selectedSupplier?.id
    };
    await this.inspectionService.loadList(prms);

    this.setTotals();
  }
}
