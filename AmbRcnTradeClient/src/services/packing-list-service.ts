import { IContainer } from "interfaces/shipping/IContainer";
import { fixAspNetCoreDate } from "./../core/helpers";
import { IRemoveContainerFromPackingListRequest } from "./../interfaces/shipping/IRemoveContainerFromPackingListRequest";
import { IPackingList } from "./../interfaces/shipping/IPackingList";
import { HttpClient } from "aurelia-fetch-client";
import { Router } from "aurelia-router";
import { Store } from "aurelia-store";
import { IState } from "store/state";
import { FetchService } from "./fetch-service";
import { noOpAction } from "./no-op-action";
import _ from "lodash";
import { autoinject } from "aurelia-dependency-injection";
@autoinject
export class PackingListService extends FetchService {
  constructor(
    http: HttpClient,
    store: Store<IState>,
    router: Router
  ) {
    super("api/packingList", http, store, router);

    store.registerAction("packingListEditAction", packingListEditAction);
    store.registerAction("packingListListAction", packingListListAction);
    store.registerAction("packingListUnallocatedContainersAction", packingListUnallocatedContainersAction);
  }

  public async createPackingList() {
    return super.get<IPackingList>([this.currentCompanyIdQuery()], "create", packingListEditAction);
  }

  public async load(id: string) {
    return await super.get<IPackingList>(id, "load", packingListEditAction);
  }

  public async save(packingList: IPackingList) {
    return await super.post<IPackingList>(packingList, "save", packingListEditAction);
  }

  public async loadList() {
    return await super.getMany<IPackingList>([this.currentCompanyIdQuery()], "loadList", packingListListAction);
  }

  public async deletePackingList(id: string) {
    return await super.delete(id, "delete", noOpAction);
  }

  public async removeContainerFromPackingList(request: IRemoveContainerFromPackingListRequest) {
    return await super.post<IPackingList>(request, "removeContainerFromPackingList", packingListEditAction);
  }

  public async getNonAllocatedContainers() {
    return await super.getMany<IContainer[]>([this.currentCompanyIdQuery()], "getNonAllocatedContainers", packingListUnallocatedContainersAction);
  }
}

export function packingListEditAction(state: IState, packingList: IPackingList) {
  packingList.date = fixAspNetCoreDate(packingList.date, false);

  const newState = _.cloneDeep(state);
  newState.packingList.current = packingList;
  return newState;
}

export function packingListListAction(state: IState, packingLists: IPackingList[]) {
  const newState = _.cloneDeep(state);
  newState.packingList.list = packingLists;
  return newState;
}

export function packingListUnallocatedContainersAction(state: IState, containers: IContainer[]) {
  const newState = _.cloneDeep(state);
  newState.packingList.unallocatedContainers = containers;
  return newState;
}
