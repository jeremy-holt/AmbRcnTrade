import { encodeParams } from "./../../core/helpers";
import { Router } from "aurelia-router";
import { CustomerService } from "./../../services/customer-service";
import { IListItem } from "./../../interfaces/IEntity";
import { IPurchaseListItem } from "./../../interfaces/purchases/IPurchaseListItem";
import { PurchaseService } from "./../../services/purchase-service";
import { autoinject, observable } from "aurelia-framework";
import { connectTo } from "aurelia-store";
import { IState } from "store/state";
import _ from "lodash";

@autoinject
@connectTo()
export class PurchaseList {
  @observable protected state: IState = undefined;
  protected list: IPurchaseListItem[] = [];
  protected suppliers: IListItem[] = [];
  @observable protected selectedSupplier: IListItem = undefined!;

  constructor(
    private purchaseService: PurchaseService,
    private customerService: CustomerService,
    private router: Router
  ) { }

  protected stateChanged(state: IState) {
    this.list = state.purchase.list;

    this.suppliers = _.cloneDeep(state.userFilteredCustomers);
    this.suppliers.unshift({ id: null, name: "[Select]" });
  }

  protected async activate() {
    await this.customerService.loadCustomersForAppUserList();    
    // await this.purchaseService.loadList(null);
  }

  protected async selectedSupplierChanged(){
    await this.purchaseService.loadList(this.selectedSupplier.id);
  }

  protected addPurchase(){
    this.router.navigateToRoute("purchaseEdit");
  }

  protected encode(value: string){
    return encodeParams(value);
  }
}
