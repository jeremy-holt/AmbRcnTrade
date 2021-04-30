import { observable } from "aurelia-binding";
import { autoinject } from "aurelia-dependency-injection";
import { Router } from "aurelia-router";
import { connectTo } from "aurelia-store";
import { CUSTOMER_GROUPS } from "constants/app-constants";
import { decodeParams, encodeParams } from "core/helpers";
import { ICustomer } from "core/interfaces/ICustomer";
import { IParamsId } from "core/interfaces/IParamsId";
import _ from "lodash";
import { AdminService } from "services/admin-service";
import { CustomerService } from "services/customer-service";
import { isInRole } from "services/role-service";
import { IState } from "store/state";

@autoinject
@connectTo()
export class AddressBook {
  public model: ICustomer = undefined!;
  public list: ICustomer[] = [];
  public state: IState = undefined!;
  public customerGroups = CUSTOMER_GROUPS;

  @observable public selectedCustomer: ICustomer = undefined!;

  constructor(
    private adminService: AdminService,
    private customerService: CustomerService,
    private router: Router
  ) { }

  protected stateChanged(state: IState): void {
    this.list = _.cloneDeep(state.customer.list);

    this.model = _.cloneDeep(state.customer.current);

    this.customerGroups = _.cloneDeep(CUSTOMER_GROUPS);
    this.customerGroups.unshift({ id: null, name: "[Select group]" });
  }

  protected async activate(params: IParamsId): Promise<void> {
    await this.customerService.loadAllCustomers();

    if (this.canAddUsers) {
      await this.adminService.loadUsersList();
    }

    if (params.id) {
      await this.customerService.loadCustomer(decodeParams(params.id) as string);
    }
  }

  protected get caption() {
    return !this.model?.id ? "Address book - New Customer" : "Address book";
  }

  protected get cardCaption() {
    return this.model?.id ? this.model?.name : "New Customer";
  }

  protected removeAppUser(index: number) {
    this.model.users.splice(index, 1);
  }


  protected async save(): Promise<void> {
    await this.customerService.saveCustomer(this.model);
    this.router.navigateToRoute("customerEdit", { id: encodeParams(this.model.id) }, { replace: true, trigger: false });
    await this.customerService.loadAllCustomers();
  }

  protected async addNewCustomer(): Promise<void> {
    await this.customerService.createCustomer();
    this.selectedCustomer = undefined!;
  }

  protected selectedCustomerChanged(newValue: ICustomer): void {
    this.model = _.cloneDeep(newValue);

    if (newValue?.id === null) {
      this.model.name = null;
    }
    if (newValue) {
      this.router.navigateToRoute("customerEdit", { id: encodeParams(newValue.id) }, { replace: true, trigger: false });
    }
  }

  protected bind() {
    this.selectedCustomer = this.list.find(c => c.id === this.model?.id) as ICustomer;
  }

  protected get canAddUsers() {
    return isInRole(["admin"], this.state);
  }

  protected get canAddAccountCodes() {
    return isInRole(["admin"], this.state);
  }

  protected get canSave() {
    return this.model?.name?.length > 3 && this.model?.companyName?.length > 3;
  }

  protected selectCustomer(item: ICustomer) {
    this.model = item;
  }
}
