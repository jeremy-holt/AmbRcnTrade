import { HttpClient } from "aurelia-fetch-client";
import { autoinject } from "aurelia-framework";
import { Router } from "aurelia-router";
import { Store } from "aurelia-store";
import _ from "lodash";
import { IState } from "../store/state";
import { IAppUserPassword } from "../interfaces/IAppUserPassword";
import { ICustomerUserListItem } from "../interfaces/ICustomerUserListItem";
import { IListItem } from "../interfaces/IEntity";
import { IPayload } from "../interfaces/IPayload";
import { FetchService } from "./fetch-service";
import { noOpAction } from "./no-op-action";

@autoinject
export class AppUserService extends FetchService {

  constructor(
    http: HttpClient,
    store: Store<IState>,
    router: Router
  ) {
    super("api/appUser", http, store, router);

    store.registerAction("userCompaniesAction", userCompaniesAction);
    store.registerAction("userCustomersAction", userCustomersAction);
    store.registerAction("customerAndUsersListAction", customerAndUsersListAction);
  }

  public async getCompaniesForUser(user: IPayload) {
    return super.post({ companyIds: user.companies }, "getCompaniesForUser", userCompaniesAction);
  }

  public async listCustomersAndUsers() {
    return await super.get([super.currentCompanyIdQuery()], "listCustomersAndUsers", customerAndUsersListAction);
  }

  public async saveAppUsersPasswords(model: IAppUserPassword[]) {
    return await super.post(model, "saveAppUsersPasswords", noOpAction);
  }

  public async loadAppUsersPasswords() {
    return await super.getData<IAppUserPassword[]>([], "loadAppUsersPasswords");
  }
}

export function userCompaniesAction(state: IState, response: IListItem[]) {
  const newState = _.cloneDeep(state);
  newState.userCompanies = response;
  return newState;
}

export function userCustomersAction(state: IState, response: string[]) {
  const newState = _.cloneDeep(state);
  newState.userCustomers = response;
  return newState;
}

export function customerAndUsersListAction(state: IState, response: ICustomerUserListItem[]) {
  const newState = _.cloneDeep(state);
  newState.customer.usersList = response;
  return newState;
}

