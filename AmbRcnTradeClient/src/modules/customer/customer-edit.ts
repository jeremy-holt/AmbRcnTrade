import { CUSTOMER_GROUPS } from "./../../constants/app-constants";
import { observable } from "aurelia-binding";
import { autoinject } from "aurelia-framework";
import { Router } from "aurelia-router";
import { connectTo } from "aurelia-store";
import { CURRENCIES_LIST } from "constants/app-constants";
import { decodeParams, encodeParams } from "core/helpers";
import { IAppUserListItem } from "interfaces/IAppUser";
import { ICustomer } from "interfaces/ICustomer";
import { IParamsId } from "interfaces/IParamsId";
import _ from "lodash";
import { AdminService } from "services/admin-service";
import { IState } from "store/state";
import { CustomerService } from "../../services/customer-service";

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

    this.model = state.customer.current;
    this.selectedCustomer = this.list.find(c => c.id === this.model?.id) as ICustomer;
    this.appUsersList = _.cloneDeep(state.admin.user.list);
    this.appUsersList.unshift({ id: "", name: "[Select]" } as IAppUserListItem);

    this.customerGroups=_.cloneDeep(CUSTOMER_GROUPS);
    this.customerGroups.unshift({id:null, name:"[Select group]"});
  }

  protected async activate(params: IParamsId): Promise<void> {    
    await this.customerService.loadAllCustomers();
    await this.adminService.loadUsersList();

    if (params.id) {
      await this.customerService.loadCustomer(decodeParams(params.id) as string);
    }
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
    await this.customerService.loadAllCustomers();
    this.selectedCustomer = this.list.find(c => c.name === this.model.name) as ICustomer;
  }

  protected async addNewCustomer(): Promise<void> {
    await this.customerService.createCustomer();
    this.selectedCustomer = this.list[0];
  }

  protected selectedCustomerChanged(newValue: ICustomer): void {
    this.model = newValue;

    if (newValue) {
      this.router.navigateToRoute("customerEdit", { id: encodeParams(newValue.id) }, { replace: true, trigger: false });
    }
  }

}
