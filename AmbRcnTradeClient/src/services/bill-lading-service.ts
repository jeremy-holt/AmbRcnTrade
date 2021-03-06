import { HttpClient } from "aurelia-fetch-client";
import { autoinject } from "aurelia-framework";
import { Router } from "aurelia-router";
import { Store } from "aurelia-store";
import { IBillLadingContainersRequest } from "interfaces/shipping/IBillLadingContainersRequest";
import { IBillLadingListItem } from "interfaces/shipping/IBillLadingListItem";
import _ from "lodash";
import { QueryId } from "core/services/QueryId";
import { IState } from "store/state";
import { encodeParams, fixAspNetCoreDate } from "./../core/helpers";
import { IAddContainersToBillLadingRequest } from "./../interfaces/shipping/IAddContainersToBillLadingRequest";
import { IBillLading } from "./../interfaces/shipping/IBillLading";
import { IContainer } from "./../interfaces/shipping/IContainer";
import { IMoveBillLadingRequest } from "./../interfaces/shipping/IMoveBillLadingRequest";
import { FetchService } from "core/services/fetch-service";
import { GetUrlService } from "core/services/get-url-service";
import { noOpAction } from "core/services/no-op-action";
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
    return await super.post<IBillLading>(billLading, "save", billLadingEditAction);
  }

  public async load(id: string) {
    return await super.get<IBillLading>(id, "load", billLadingEditAction);
  }

  public async loadList() {
    return await super.getMany<IBillLadingListItem[]>([super.currentCompanyIdQuery()], "loadList", billLadingListAction);
  }

  public async removeContainersFromBillLading(billLadingId: string, containerIds: string[]) {
    const request: IBillLadingContainersRequest = { billLadingId, containerIds };
    return await super.post(request, "removeContainersFromBillLading", noOpAction);
  }

  public async getNotLoadedContainers() {
    return await super.getMany<IContainer[]>([super.currentCompanyIdQuery()], "getNotLoadedContainers", billLadingNotLoadedContainersAction);
  }

  public async createBillLading(vesselId: string) {
    return await super.get<IBillLading>([this.currentCompanyIdQuery(), new QueryId("vesselId", vesselId)], "create", billLadingEditAction);
  }

  public async moveBillLadingToVessel(billLadingId: string, fromVesselId: string, toVesselId: string) {
    const request: IMoveBillLadingRequest = { billLadingId, fromVesselId, toVesselId };
    return await super.post<IMoveBillLadingRequest>(request, "moveBillLadingToVessel", noOpAction);
  }

  public async addContainersToBillLading(billLadingId: string, vesselId: string, containerIds: string[]) {
    const request: IAddContainersToBillLadingRequest = { billLadingId, vesselId, containerIds };    
    return await super.post<IBillLading>(request, "addContainersToBillLading", noOpAction);
  }

  public async deleteBillLading(vesselId: string, billLadingId: string) {
    return await super.post({ vesselId, billLadingId }, "deleteBillLading", noOpAction);
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
  billLading.blDate=fixAspNetCoreDate(billLading.blDate,false);
  billLading.documents.forEach(doc=>{
    doc.received=fixAspNetCoreDate(doc.received,false);
    doc.submitted=fixAspNetCoreDate(doc.submitted,false);
  });
  
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

