import { Router } from "aurelia-router";
import { encodeParams } from "./../../core/helpers";
import { CustomerGroupService } from "./../../services/customer-group-service";
import { IParamsId } from "./../../interfaces/IParamsId";
import { ICustomerGroup } from "./../../interfaces/ICustomerGroup";
import { autoinject, observable } from "aurelia-framework";
import { connectTo } from "aurelia-store";
import { IState } from "store/state";
import _ from "lodash";

@autoinject
@connectTo()
export class CustomerGroupEdit {
  @observable protected state: IState = undefined!;
  protected model: ICustomerGroup = undefined;
  protected list: ICustomerGroup[] = undefined;
  @observable protected selectedCustomerGroup: ICustomerGroup = undefined!

  protected constructor(
    private customerGroupService: CustomerGroupService,
    private router: Router
  ) { }

  protected stateChanged(state: IState) {
    this.list = _.cloneDeep(state.customerGroup.list);
    this.list.unshift({ id: null, name: "[Select]" } as ICustomerGroup);

    this.model = _.cloneDeep(state.customerGroup.current);
  }

  protected async activate(prms: IParamsId) {
    await this.customerGroupService.loadList();

    if (prms?.id) {
      await this.customerGroupService.load(prms.id);
    } else {
      await this.customerGroupService.createCustomerGroup();
    }
  }

  protected async selectedCustomerGroupChanged(value: ICustomerGroup) {
    //   this.model = { id: null, companyId: value.companyId, name: undefined };

    //   await this.customerGroupService.load(encodeParams(value.id));

    this.router.navigateToRoute("customerGroupEdit", { id: encodeParams(value.id) });
  }

  protected bind() {
    this.selectedCustomerGroup = this.list.find(c => c.id === this.model.id);
  }

  protected get canSave() {
    return this.model?.name?.length > 0;
  }

  protected async save() {
    if (this.canSave) {
      await this.customerGroupService.save(this.model);
      await this.customerGroupService.loadList();
    }
  }

  protected async addCustomerGroup() {
    this.selectedCustomerGroup = this.list.find(c => c.id === null);
    await this.customerGroupService.createCustomerGroup();
  }
}
