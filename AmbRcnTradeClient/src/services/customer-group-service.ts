import { HttpClient } from "aurelia-fetch-client";
import { autoinject } from "aurelia-framework";
import { Router } from "aurelia-router";
import { Store } from "aurelia-store";
import _ from "lodash";
import { IState } from "store/state";
import { ICustomerGroup } from "./../interfaces/ICustomerGroup";
import { FetchService } from "./fetch-service";

@autoinject
export class CustomerGroupService extends FetchService {
  constructor(
    http: HttpClient,
    store: Store<IState>,
    router: Router
  ) {
    super("api/customerGroup", http, store, router);

    store.registerAction("customerGroupEditAction", customerGroupEditAction);
    store.registerAction("customerGroupListAction", customerGroupListAction);
  }

  public async load(id: string) {
    return super.get<ICustomerGroup>(id, "load", customerGroupEditAction);
  }

  public async save(customrGroup: ICustomerGroup) {
    return super.post<ICustomerGroup>(customrGroup, "save", customerGroupEditAction);
  }

  public async loadList() {
    return super.getMany<ICustomerGroup[]>([super.currentCompanyIdQuery()], "loadList", customerGroupListAction);
  }

  public async createCustomerGroup() {
    return super.get<ICustomerGroup>([super.currentCompanyIdQuery()], "create", customerGroupEditAction);
  }
}

export function customerGroupEditAction(state: IState, customerGroup: ICustomerGroup) {
  const newState = _.cloneDeep(state);
  newState.customerGroup.current = customerGroup;
  return newState;
}

export function customerGroupListAction(state: IState, customerGroups: ICustomerGroup[]) {
  const newState = _.cloneDeep(state);
  newState.customerGroup.list = customerGroups;
  return newState;
}
