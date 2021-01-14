import { HttpClient } from "aurelia-fetch-client";
import { autoinject } from "aurelia-framework";
import { Router } from "aurelia-router";
import { Store } from "aurelia-store";
import _ from "lodash";
import { ICompany } from "../interfaces/ICompany";
import { IRoleNameItem } from "../interfaces/IRoleNameItem";
import { QueryId } from "../models/QueryId";
import { IState } from "../store/state";
import { IAppUser, IAppUserListItem } from "./../interfaces/IAppUser";
import { ICompanyListItem } from "./../interfaces/ICompany";
import { IEntity } from "./../interfaces/IEntity";
import { FetchService } from "./fetch-service";
import { noOpAction } from "./no-op-action";

@autoinject
export class AdminService extends FetchService {

  constructor(
    http: HttpClient,
    store: Store<IState>,
    router: Router
  ) {
    super("api/Admin", http, store, router);

    store.registerAction("adminCompanyAction", adminCompanyAction);
    store.registerAction("adminCompanyListAction", adminCompanyListAction);
    store.registerAction("adminUserAction", adminUserAction);
    store.registerAction("adminUserListAction", adminUserListAction);
    store.registerAction("adminRoleNamesAction", adminRoleNamesAction);
    store.registerAction("adminCompanyNameAction", adminCompanyNameAction);
  }

  public async saveCompany(model: ICompany) {
    return await super.post<ICompany>(model, "saveCompany", adminCompanyAction);
  }

  public async loadCompany(id: string) {
    return await super.get(id, "loadCompany", adminCompanyAction);
  }

  public async createEmptyCompany() {
    return await super.get("", "createEmptyCompany", adminCompanyAction);
  }

  public async loadCompaniesList() {
    return await super.getMany<ICompanyListItem>([], "loadCompaniesList", adminCompanyListAction);
  }

  public async deleteCompany(id: string) {
    return await super.delete<ICompany>(id, "deleteCompany", adminCompanyAction);
  }

  public async getCompanyName(id: string) {
    return await super.get(id, "getCompanyName", adminCompanyNameAction);
  }

  public async deleteUser(id: string) {
    return await super.delete<IAppUser>(id, "deleteUser", adminUserAction);
  }

  public async createAdminUser(){
    return await super.post({companyId: null},"createAdminUser",noOpAction);
  }

  public async saveUser(model: IAppUser) {
    if (model.id) {
      return await super.post<IAppUser>(model, "saveUser", adminUserAction);
    }
    return await super.post<IAppUser>(model, "createAppUser", adminUserAction);
  }

  public async loadUser(id: string) {
    return await super.get(id, "loadUser", adminUserAction);
  }

  public async createEmptyUser() {
    return await super.get("", "createEmptyAppUser", adminUserAction);
  }

  public async loadUsersList() {
    return await super.getMany<IAppUserListItem>([], "loadUsersList", adminUserListAction);
  }

  public async getRoleNames() {
    return await super.getMany<IRoleNameItem>([], "getRoleNames", adminRoleNamesAction);
  }

  public async removeDemonstrationCompany(companyId: string) {
    return await super.delete([new QueryId("companyId", companyId)], "RemoveDemonstrationCompany", noOpAction);
  }
}

export function adminCompanyAction(state: IState, response: ICompany) {
  const newState = _.cloneDeep(state);
  newState.admin.company.current = response;
  return newState;
}

export function adminCompanyListAction(state: IState, response: ICompanyListItem[]) {
  const newState = _.cloneDeep(state);
  newState.admin.company.list = response;
  return newState;
}

export function adminUserAction(state: IState, response: IAppUser) {
  const newState = _.cloneDeep(state);
  newState.admin.user.current = response;
  return newState;
}

export function adminUserListAction(state: IState, response: IAppUserListItem[]) {
  const newState = _.cloneDeep(state);
  newState.admin.user.list = response;
  return newState;
}

export function adminRoleNamesAction(state: IState, response: IRoleNameItem[]) {
  const newState = _.cloneDeep(state);
  newState.admin.roleNames = response;
  return newState;
}

export function adminCompanyNameAction(state: IState, response: IEntity) {
  const newState = _.cloneDeep(state);
  newState.currentCompanyName = response.name as string;
  return newState;
}
