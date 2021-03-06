import { isInRole } from "core/services/role-service";
import { CUSTOMER_GROUPS } from "./../../constants/app-constants";
import { observable } from "aurelia-binding";
import { autoinject } from "aurelia-framework";
import { Router } from "aurelia-router";
import { connectTo } from "aurelia-store";
import { CURRENCIES_LIST } from "constants/app-constants";
import { decodeParams, encodeParams } from "core/helpers";
import { IAppUserListItem } from "core/interfaces/IAppUser";
import { ICustomer } from "core/interfaces/ICustomer";
import { IParamsId } from "core/interfaces/IParamsId";
import _ from "lodash";
import { AdminService } from "core/services/admin-service";
import { IState } from "store/state";
import { CustomerService } from "../../core/services/customer-service";

@autoinject
@connectTo()
export class CustomerEdit {
  public model: ICustomer = undefined!;
  public list: ICustomer[] = [];
  public state: IState = undefined!;
  public customerBlock: string = undefined!;
  public customerInLine: string = undefined!;
  public currenciesList = CURRENCIES_LIST;
  public appUsersList: IAppUserListItem[] = [];
  public selectedAppUser: IAppUserListItem = undefined!;
  public customerGroups = CUSTOMER_GROUPS;

  @observable public selectedCustomer: ICustomer = undefined!;

  constructor(
    private adminService: AdminService,
    private customerService: CustomerService,
    private router: Router
  ) { }

  protected stateChanged(state: IState): void {
    this.list = _.cloneDeep(state.customer.list);

    const nullField: Partial<ICustomer> = { id: null, name: "[Select]" };
    this.list.unshift(nullField as ICustomer);

    this.model = _.cloneDeep(state.customer.current);

    this.appUsersList = _.cloneDeep(state.admin.user.list);
    this.appUsersList.unshift({ id: "", name: "[Select]" } as IAppUserListItem);

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

  protected addAppUser() {
    this.model.users.push({ appUserId: this.selectedAppUser.id as string, name: this.selectedAppUser.name });
  }

  protected get canAddAppuser() {
    return this.selectedAppUser?.id && !this.model?.users?.find(c => c.appUserId === this.selectedAppUser.id);
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
    this.selectedCustomer = this.list[0];
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

}
