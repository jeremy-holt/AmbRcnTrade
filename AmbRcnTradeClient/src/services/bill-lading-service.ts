import { HttpClient } from "aurelia-fetch-client";
import { autoinject } from "aurelia-framework";
import { Router } from "aurelia-router";
import { Store } from "aurelia-store";
import { IBillLadingContainersRequest } from "interfaces/shipping/IBillLadingContainersRequest";
import { IBillLadingListItem } from "interfaces/shipping/IBillLadingListItem";
import _ from "lodash";
import { QueryId } from "models/QueryId";
import { IState } from "store/state";
import { encodeParams } from "./../core/helpers";
import { IAddContainersToBillLadingRequest } from "./../interfaces/shipping/IAddContainersToBillLadingRequest";
import { IBillLading } from "./../interfaces/shipping/IBillLading";
import { IContainer } from "./../interfaces/shipping/IContainer";
import { IMoveBillLadingRequest } from "./../interfaces/shipping/IMoveBillLadingRequest";
import { FetchService } from "./fetch-service";
import { GetUrlService } from "./get-url-service";
import { noOpAction } from "./no-op-action";
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
    const request: IBillLadingContainersRequest = { billLadingId, containerIds };
    return super.post(request, "removeContainersFromBillLading", noOpAction);
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

  public async addContainersToBillLading(billLadingId: string, containerIds: string[]) {
    const request: IAddContainersToBillLadingRequest = { billLadingId, containerIds };
    return super.post<IBillLading>(request, "addContainersToBillLading", noOpAction);
  }

  public async deleteBillLading(vesselId: string, billLadingId: string) {
    return super.post({ vesselId, billLadingId }, "deleteBillLading", noOpAction);
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

