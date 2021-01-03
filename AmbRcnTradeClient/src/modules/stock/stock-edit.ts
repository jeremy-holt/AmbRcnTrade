import { encodeParams } from "core/helpers";
import { autoinject, observable } from "aurelia-framework";
import { Router } from "aurelia-router";
import { connectTo } from "aurelia-store";
import { IListItem } from "interfaces/IEntity";
import { IParamsId } from "interfaces/IParamsId";
import { IStock } from "interfaces/stocks/IStock";
import _ from "lodash";
import { IState } from "store/state";
import { CustomerService } from "./../../services/customer-service";
import { StockService } from "./../../services/stock-service";

@autoinject
@connectTo()
export class StockEdit {
  @observable protected state: IState = undefined!;
  protected model: IStock = undefined!;
  protected locations: IListItem[] = [];
  protected suppliers: IListItem[] = [];
  // protected approvalChecked: Approval = null;

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
    if (this.model && this.locations?.length > 0) {
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
    return this.model?.locationId && this.model?.supplierId && this.model?.bags > 0;
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

  protected encode(value: string){
    return encodeParams(value);
  }

  protected listItemMatcher = (a: IListItem, b: IListItem) => a?.id === b?.id;
}
