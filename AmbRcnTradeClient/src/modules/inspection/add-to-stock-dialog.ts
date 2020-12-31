import { IStockListItem } from "./../../interfaces/stocks/IStockListItem";
import { StockService } from "./../../services/stock-service";
import { DialogController } from "aurelia-dialog";
import { autoinject, observable } from "aurelia-framework";
import { Router } from "aurelia-router";
import { connectTo } from "aurelia-store";
import { DATEFORMAT } from "constants/app-constants";
import { IInspection } from "interfaces/inspections/IInspection";
import _ from "lodash";
import moment from "moment";
import { IState } from "store/state";
import { IListItem } from "./../../interfaces/IEntity";
import { IMoveInspectionToStockRequest } from "./../../interfaces/stockManagement/IMoveInspectionToStockRequest";
import { CustomerService } from "./../../services/customer-service";

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

  constructor(
    protected controller: DialogController,
    private customerService: CustomerService,
    private stockService: StockService,
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

  protected async activate(model: { inspection: IInspection }) {
    await this.customerService.loadCustomersForAppUserList();
    await this.stockService.loadStockList(null, null);
    this.inspection = model.inspection;

    this.model.bags = this.inspection.bags - this.inspection.stockReferences.reduce((a, b) => a += b.bags, 0);
    this.model.inspectionId = this.inspection.id;
    this.model.date = moment().format(DATEFORMAT);
  }

  protected bind() {
    this.selectedLocation = this.locations.find(c => c.id === this.inspection.supplierId);
  }

  protected get canCreate(): boolean {
    return this.model?.bags > 0 && this.model?.date && this.model?.locationId !== null &&
      (this.newStockItem || this.stockList.some(x => x.selected));
  }

  protected selectStockRow(item: IStockListItem) {
    item.selected = !item.selected;
    this.selectedLocation = this.locations.find(c => c.id === item.locationId);
    if (item.selected) {
      this.newStockItem = false;
    }
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
      date: this.model.date,
      locationId: this.selectedLocation.id,
      lotNo: this.newStockItem ? 0 : this.stockList.find(c => c.selected)?.lotNo
    };
  }

  protected async createStockAllocation() {
    await this.controller.ok(this.requestModel);
  }
}
