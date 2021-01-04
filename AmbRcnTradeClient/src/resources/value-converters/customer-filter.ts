import { Subscription } from "rxjs";
import { autoinject, observable } from "aurelia-framework";
import { connectTo, Store } from "aurelia-store";
import { IState } from "store/state";
import { ICustomerListItem } from "../../interfaces/ICustomerListItem";
import { CustomerGroupService } from "../../services/customer-group-service";

@autoinject
@connectTo()
export class CustomerFilterValueConverter {
  @observable protected state: IState = undefined!;
  private subscriptions: Subscription[] = [];

  public toView(list: ICustomerListItem[], filter: string) {
    if (filter?.length === 0) {
      return list;
    }

    if (this.state === undefined!) {
      return [];
    }

    const id = this.state.customerGroup.list.find(c => c.name.toLowerCase() === filter.toLowerCase())?.id;
    return list.filter(c => c.id === null || c?.customerGroupId === id);
  }

  constructor(
    private customerGroupService: CustomerGroupService,
    store: Store<IState>
  ) {
    this.subscriptions.push(store.state.subscribe(state => this.state = state));
  }

  protected detached() {
    this.subscriptions.forEach(c => c.unsubscribe());
  }

  protected async bind() {
    if (this.state?.customerGroup?.list?.length === 0) {
      await this.customerGroupService.loadList();
    }
  }
}
