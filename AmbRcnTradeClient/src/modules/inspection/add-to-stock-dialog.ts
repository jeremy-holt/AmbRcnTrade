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
  @observable selectedLocation: IListItem = undefined!;

  public model: IMoveInspectionToStockRequest = {} as IMoveInspectionToStockRequest;
  public inspection: IInspection

  constructor(
    protected controller: DialogController,
    private customerService: CustomerService,
    private router: Router
  ) { }

  protected stateChanged(state: IState) {
    this.locations = _.cloneDeep(state.userFilteredCustomers);
    this.locations.unshift({ id: null, name: "[Select]" });
  }

  protected selectedLocationChanged() {
    if (this.model) {
      this.model.locationId = this.selectedLocation.id;
    }
  }

  protected async activate(model: { inspection: IInspection }) {
    await this.customerService.loadCustomersForAppUserList();
    this.inspection = model.inspection;

    this.model.bags = this.inspection.bags;
    this.model.inspectionId = this.inspection.id;
    this.model.date = moment().format(DATEFORMAT);
  }

  protected bind() {
    this.selectedLocation = this.locations.find(c => c.id === this.inspection.supplierId);
  }

  protected get canCreate(): boolean {
    return this.model?.bags > 0 && this.model?.date && this.model?.locationId !== null;
  }

  protected async createStockAllocation() {    
    await this.controller.ok(this.model);    
  }
}
