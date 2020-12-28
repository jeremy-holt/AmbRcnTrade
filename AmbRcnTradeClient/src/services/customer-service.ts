import { HttpClient } from "aurelia-fetch-client";
import { autoinject } from "aurelia-framework";
import { Router } from "aurelia-router";
import { Store } from "aurelia-store";
import { IListItem } from "interfaces/IEntity";
import _ from "lodash";
import { QueryId } from "models/QueryId";
import { IState } from "store/state";
import { ICustomer } from "./../interfaces/ICustomer";
import { FetchService } from "./fetch-service";
import { isInRole } from "./role-service";

@autoinject
export class CustomerService extends FetchService {
  constructor(
    http: HttpClient,
    router: Router,
    store: Store<IState>
  ) {
    super("api/customer", http, store, router);

    store.registerAction("customerEditAction", customerEditAction);
    store.registerAction("customerListAction", customerListAction);
    store.registerAction("customerAppUserListAction", customerAppUserListAction);
  }

  public async loadCustomer(id: string) {
    return super.get(id, "load", customerEditAction);
  }

  public async saveCustomer(model: ICustomer) {
    return super.post(model, "save", customerEditAction);
  }

  public async loadCustomerList() {
    return super.getMany<ICustomer>([super.currentCompanyIdQuery()], "loadCustomerList", customerListAction);
  }

  public async createCustomer() {
    return super.get([], "create", customerEditAction);
  }

  public async loadCustomersForAppUserList() {
    const state = this.getCurrentState();
    const appUserId = isInRole(["admin", "user", "inspector"], state) ? null : state.user.id;

    return super.getMany<IListItem>([
      super.currentCompanyIdQuery(),
      new QueryId("appUserId", appUserId)
    ], "loadCustomerListForAppUser", customerAppUserListAction);
  }
}

export function customerEditAction(state: IState, customer: ICustomer) {
  const newState = _.cloneDeep(state);
  newState.customer.current = customer;
  return newState;
}

export function customerListAction(state: IState, list: ICustomer[]) {
  const newsState = _.cloneDeep(state);
  newsState.customer.list = list;
  return newsState;
}

export function customerAppUserListAction(state: IState, list: IListItem[]) {
  const newState = _.cloneDeep(state);
  newState.userFilteredCustomers = list;
  return newState;
}
