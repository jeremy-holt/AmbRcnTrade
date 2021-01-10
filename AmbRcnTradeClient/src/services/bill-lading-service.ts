import { encodeParams } from "./../core/helpers";
import { GetUrlService } from "./get-url-service";
import { IMoveBillLadingRequest } from "./../interfaces/shipping/IMoveBillLadingRequest";
import { IContainer } from "./../interfaces/shipping/IContainer";
import { HttpClient, json } from "aurelia-fetch-client";
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
import { FetchRoute } from "requests/FetchRoute";
@autoinject

export class BillLadingService extends FetchService {
  constructor(
    http: HttpClient,
    store: Store<IState>,
    router: Router,
    private urlService: GetUrlService
  ) {

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

  public async moveBillLadingToVessel(billLadingId: string, fromVesselId: string, toVesselId: string) {
    const request: IMoveBillLadingRequest = { billLadingId, fromVesselId, toVesselId };
    return super.post<IMoveBillLadingRequest>(request, "moveBillLadingToVessel", noOpAction);
  }

  public async getDraftBillOfLading(vesselId: string, billLadingId: string) {
    const params = [
      new QueryId("vesselId", encodeParams(vesselId)),
      new QueryId("billLadingId", encodeParams(billLadingId))
    ];

    const url = this.url(params, "getDraftBillOfLading");

    const response = await this.http.fetch(url);
    const blob = await response.blob();
    const objectUrl = URL.createObjectURL(blob);

    return objectUrl;
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
