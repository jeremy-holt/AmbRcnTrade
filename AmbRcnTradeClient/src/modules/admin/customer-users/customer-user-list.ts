
import { UserService } from "./../../../services/user-service";
import { autoinject, observable } from "aurelia-framework";
import { connectTo } from "aurelia-store";
import { IState } from "store/state";
import { ICustomerUserListItem } from "interfaces/ICustomerUserListItem";

@autoinject
@connectTo()
export class CustomerUsersList {
  @observable public state: IState = undefined!;
  public list: ICustomerUserListItem[] = [];

  constructor(
    private userService: UserService
  ) { }

  protected async activate() {
    await this.userService.listCustomersAndUsers();
  }

  protected stateChanged(state: IState) {
    this.list = state.customer.usersList;
  }
}
