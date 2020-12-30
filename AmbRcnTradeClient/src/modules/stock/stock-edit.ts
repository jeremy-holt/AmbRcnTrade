import { Approval } from "constants/app-constants";
import { IListItem } from "interfaces/IEntity";
import { CustomerService } from "./../../services/customer-service";
import { Router } from "aurelia-router";
import { StockService } from "./../../services/stock-service";
import { IParamsId } from "interfaces/IParamsId";
import { autoinject, observable } from "aurelia-framework";
import { connectTo } from "aurelia-store";
import { IStock } from "interfaces/stocks/IStock";
import { IState } from "store/state";
import _ from "lodash";

@autoinject
@connectTo()
export class StockEdit {
  @observable protected state: IState = undefined!;
  protected model: IStock = undefined!;
  protected locations: IListItem[] = [];
  protected suppliers: IListItem[] = [];
  protected approvalChecked: Approval = null;

  @observable protected selectedLocation: IListItem = undefined!;
  @observable protected selectedSupplier: IListItem = undefined!;

  constructor(
    private stockService: StockService,
    private customerService: CustomerService,
    private router: Router
  ) { }

  protected stateChanged(state: IState) {
    this.locations = _.cloneDeep(state.userFilteredCustomers);
    this.suppliers = _.cloneDeep(state.userFilteredCustomers);

    this.locations.unshift({ id: null, name: "[Select]" });
    this.suppliers.unshift({ id: null, name: "[Select]" });

    this.model = _.cloneDeep(state.stock.current);
    if (this.model) {
      this.selectedLocation = this.locations.find(c => c.id === this.model.locationId);
      this.selectedSupplier = this.suppliers.find(c => c.id === this.model.supplierId);
    }
  }

  protected async activate(params: IParamsId) {
    await this.customerService.loadCustomersForAppUserList();

    if (params?.id) {
      await this.stockService.load(params.id);
    } else {
      await this.stockService.createStock();
    }
  }

  protected get canSave() {
    return true;
  }

  protected async save() {
    if (this.canSave) {
      await this.stockService.save(this.model);
    }
  }

  protected selectedLocationChanged() {
    if (this.model) {
      this.model.locationId = this.selectedLocation.id;
    }
  }

  protected selectedSupplierChanged() {
    if (this.model) {
      this.model.supplierId = this.selectedSupplier.id;
    }
  }

  // protected locationMatcher = (a: IStock, b: IListItem) => {
  //   console.log("a", a);
  //   console.log("b", b);

  //   a?.locationId === b?.id;
  // }
  // protected locationMatcher = (a: IListItem, b: IListItem) => {
  //   console.log("a", a);
  //   console.log("b", b);
  //   a?.id === b?.id;
  // }
  protected supplierMatcher = (a: IStock, b: IListItem) => a?.supplierId === b?.id;
}
