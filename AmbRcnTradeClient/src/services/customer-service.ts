import { HttpClient } from "aurelia-fetch-client";
import { autoinject } from "aurelia-framework";
import { Router } from "aurelia-router";
import { Store } from "aurelia-store";
import _ from "lodash";
import { IState } from "store/state";
import { ICustomer } from "./../interfaces/ICustomer";
import { ICustomerListItem } from "./../interfaces/ICustomerListItem";
import { FetchService } from "./fetch-service";

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

  public async loadAllCustomers() {
    return super.getMany<ICustomer>([super.currentCompanyIdQuery()], "loadAllCustomers", customerListAction);
  }

  public async createCustomer() {
    return super.get([], "create", customerEditAction);
  }

  public async loadCustomersForAppUserList() {
    return super.getMany<ICustomerListItem>([
      super.currentCompanyIdQuery()
    ], "loadCustomerListForAppUser", customerAppUserListAction);
  }

  public static Address(customer: ICustomer) {
    const lines: string[] = [];
    this.addToAddress(lines, customer?.companyName);
    this.addToAddress(lines, customer.address?.street1);
    this.addToAddress(lines, customer.address?.street2);
    this.addToAddress(lines, customer.address?.city);
    this.addToAddress(lines, customer.address?.state);
    if (customer.name.toLowerCase() !== "to order") {
      this.addToAddress(lines, `${customer.address?.country} ${customer.address?.postCode || ""}`);
    }
    this.addToAddress(lines, customer?.reference);
    this.addToAddress(lines, customer?.email);

    return lines.join("<br>");
  }

  private static addToAddress(lines: string[], value: string) {
    if (value && value.trim().length > 0) {
      lines.push(value);
    }
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

export function customerAppUserListAction(state: IState, list: ICustomerListItem[]) {
  const newState = _.cloneDeep(state);
  newState.userFilteredCustomers = list;
  return newState;
}
