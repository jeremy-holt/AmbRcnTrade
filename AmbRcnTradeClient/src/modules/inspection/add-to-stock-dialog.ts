import { DialogController } from "aurelia-dialog";
import { autoinject, observable } from "aurelia-framework";
import { Router } from "aurelia-router";
import { connectTo } from "aurelia-store";
import { DATEFORMAT } from "constants/app-constants";
import { IInspection } from "interfaces/inspections/IInspection";
import _ from "lodash";
import moment from "moment";
import { InspectionService } from "services/inspection-service";
import { IState } from "store/state";
import { IListItem } from "./../../interfaces/IEntity";
import { IMoveInspectionToStockRequest } from "./../../interfaces/stockManagement/IMoveInspectionToStockRequest";
import { IStockListItem } from "./../../interfaces/stocks/IStockListItem";
import { CustomerService } from "./../../services/customer-service";
import { StockService } from "./../../services/stock-service";

@autoinject
@connectTo()
export class AddToStockDialog {
  @observable public state: IState = undefined!;

  public locations: IListItem[] = [];
  public stockList: IStockListItem[] = [];

  @observable public selectedLocation: IListItem = undefined!;
  @observable public newStockItem = false;
  public model: IMoveInspectionToStockRequest = {} as IMoveInspectionToStockRequest;
  public inspection: IInspection
  public supplierName = "";
  protected averageBagWeightKg=0;

  constructor(
    protected controller: DialogController,
    private customerService: CustomerService,
    private stockService: StockService,
    private inspectionService: InspectionService,
    private router: Router
  ) { }

  protected stateChanged(state: IState) {
    this.locations = _.cloneDeep(state.userFilteredCustomers);
    this.locations.unshift({ id: null, name: "[Select]" });
    this.stockList = _.cloneDeep(state.stock.list);
  }

  protected selectedLocationChanged() {
    if (this.model) {
      this.model.locationId = this.selectedLocation.id;
    }
  }

  protected async activate(model: { inspection: IInspection, supplierName: string }) {
    await this.customerService.loadCustomersForAppUserList();
    await this.stockService.loadStockList(null);
    this.inspection = model.inspection;
    this.supplierName = model.supplierName;

    this.model.bags = this.inspection.bags - this.inspection.stockReferences.reduce((a, b) => a += b.bags, 0);
    this.model.weightKg = this.inspection.weightKg - this.inspection.stockReferences.reduce((a, b) => a += b.weightKg, 0);
    this.averageBagWeightKg = this.model.bags > 0 ? this.model.weightKg / this.model.bags : 0;
    this.model.inspectionId = this.inspection.id;
    this.model.date = moment().format(DATEFORMAT);
    this.model.origin = this.inspection.origin;
  }

  protected bind() {
    this.selectedLocation = this.locations.find(c => c.id === this.inspection.supplierId);
  }

  protected get canCreate(): boolean {
    return this.model?.bags > 0 && this.model?.date && this.model?.locationId !== null &&
      (this.newStockItem || this.stockList.some(x => x.selected)) &&
      !this.inspectionService.wouldExceedInspectionBags(this.inspection, this.model?.bags);
  }

  protected calcWeightKg() {
    return this.model.weightKg = this.model.bags * this.averageBagWeightKg;
  }

  protected get warningMessage() {
    return this.inspectionService.wouldExceedInspectionBags(this.inspection, this?.model.bags)
      ? `Putting ${this?.model.bags} into stock would exceed the total number of bags inspected`
      : ""!;
  }

  protected selectStockRow(item: IStockListItem) {
    item.selected = !item.selected;
    this.selectedLocation = this.locations.find(c => c.id === item.locationId);
    if (item.selected) {
      this.newStockItem = false;
    }
  }

  protected get pleaseSelectMessage(): string {
    return this.stockList.every(c => !c.selected) && !this.newStockItem ? "Please select one or several stock rows or create a new stock" : "";
  }

  protected get stockItemsSelectedCount() {
    return this.stockList.filter(c => c.selected).length;
  }

  protected newStockItemChanged() {
    if (this.newStockItem) {
      this.stockList.forEach(c => c.selected = false);
    }
  }

  protected get requestModel(): IMoveInspectionToStockRequest {
    return {
      inspectionId: this.model.inspectionId,
      bags: this.model.bags,
      weightKg: this.model.weightKg,
      date: this.model.date,
      locationId: this.selectedLocation.id,
      lotNo: this.newStockItem ? 0 : this.stockList.find(c => c.selected)?.lotNo,
      origin: this.model.origin
    };
  }

  protected async createStockAllocation() {
    await this.controller.ok(this.requestModel);
  }
}
