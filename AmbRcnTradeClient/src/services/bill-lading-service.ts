import { IContainer } from "./../interfaces/shipping/IContainer";
import { HttpClient } from "aurelia-fetch-client";
import { autoinject } from "aurelia-framework";
import { Router } from "aurelia-router";
import { Store } from "aurelia-store";
import { IBillLadingListItem } from "interfaces/shipping/IBillLadingListItem";
import _ from "lodash";
import { QueryId } from "models/QueryId";
import { IState } from "store/state";
import { IBillLading } from "./../interfaces/shipping/IBillLading";
import { FetchService } from "./fetch-service";
import { noOpAction } from "./no-op-action";
@autoinject

export class BillLadingService extends FetchService {
  constructor(
    http: HttpClient,
    store: Store<IState>,
    router: Router) {

    super("api/billLading", http, store, router);

    store.registerAction("billLadingEditAction", billLadingEditAction);
    store.registerAction("billLadingListAction", billLadingListAction);
    store.registerAction("billLadingNotLoadedContainersAction", billLadingNotLoadedContainersAction);
  }

  public async save(billLading: IBillLading) {
    return super.post<IBillLading>(billLading, "save", billLadingEditAction);
  }

  public async load(id: string) {
    return super.get<IBillLading>(id, "load", billLadingEditAction);
  }

  public async loadList() {
    return super.getMany<IBillLadingListItem[]>([super.currentCompanyIdQuery()], "loadList", billLadingListAction);
  }

  public async removeContainersFromBillLading(billLadingId: string, containerIds: string[]) {
    return super.post({ billLadingId, containerIds }, "removeContainersFromBillLading", noOpAction);
  }

  public async getNotLoadedContainers() {
    return super.getMany<IContainer[]>([super.currentCompanyIdQuery()], "getNotLoadedContainers", billLadingNotLoadedContainersAction);
  }

  public async createBillLading(vesselId: string) {
    return super.get<IBillLading>([this.currentCompanyIdQuery(), new QueryId("vesselId", vesselId)], "create", billLadingEditAction);
  }
}

export function billLadingEditAction(state: IState, billLading: IBillLading) {
  const newState = _.cloneDeep(state);
  newState.billLading.current = billLading;
  return newState;
}

export function billLadingListAction(state: IState, list: IBillLadingListItem[]) {
  const newState = _.cloneDeep(state);
  newState.billLading.list = list;
  return newState;
}

export function billLadingNotLoadedContainersAction(state: IState, notLoadedContainers: IContainer[]) {
  const newState = _.cloneDeep(state);
  newState.vessel.notLoadedContainers = notLoadedContainers;
  return newState;
}
