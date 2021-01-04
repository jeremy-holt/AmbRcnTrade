import { IVesselContainersRequest } from "./../interfaces/shipping/IVesselContainerRequest";
import { fixAspNetCoreDate } from "./../core/helpers";
import { INotLoadedContainer } from "./../interfaces/shipping/INotLoadedContainer";
import { IVesselListItem } from "./../interfaces/shipping/IVesselListItem";
import { Router } from "aurelia-router";
import { HttpClient } from "aurelia-fetch-client";
import { FetchService } from "./fetch-service";
import { autoinject } from "aurelia-framework";
import { Store } from "aurelia-store";
import { IState } from "store/state";
import _ from "lodash";
import { IVessel } from "interfaces/shipping/IVessel";
import { noOpAction } from "./no-op-action";

@autoinject
export class VesselService extends FetchService {
  constructor(
    http: HttpClient,
    store: Store<IState>,
    router: Router
  ) {
    super("api/vessel", http, store, router);

    store.registerAction("vesselEditFunction", vesselEditFunction);
    store.registerAction("vesselListAction", vesselListAction);
    store.registerAction("vesselNotLoadedContainersAction", vesselNotLoadedContainersAction);
  }

  public async save(vessel: IVessel) {
    return super.post<IVessel>(vessel, "save", vesselEditFunction);
  }

  public async load(id: string) {
    return super.get<IVessel>(id, "load", vesselEditFunction);
  }

  public async loadList() {
    return super.getMany<IVesselListItem[]>([this.currentCompanyIdQuery()], "loadList", vesselListAction);
  }

  public async getNotLoadedContainers() {
    return super.getMany<INotLoadedContainer[]>([this.currentCompanyIdQuery()], "getNotLoadedContainers", vesselNotLoadedContainersAction);
  }

  public async addContainersToVessel(request: IVesselContainersRequest) {
    return super.post(request, "addContainersToVessel", noOpAction);
  }

  public async removeContainersFromVessel(request: IVesselContainersRequest) {
    return super.post(request, "removeContainersFromVessel", noOpAction);
  }

  public async createVessel(){
    return super.get<IVessel>([this.currentCompanyIdQuery()],"create",vesselEditFunction);
  }

}

export function vesselEditFunction(state: IState, vessel: IVessel) {
  vessel.blDate = fixAspNetCoreDate(vessel.blDate, false);
  vessel.etaHistory.forEach(c => {
    c.dateUpdated = fixAspNetCoreDate(c.dateUpdated, false);
    c.eta = fixAspNetCoreDate(c.eta, false);
  });
  vessel.containers.forEach(c => {
    c.dispatchDate = fixAspNetCoreDate(c.dispatchDate, false);
    c.stuffingDate = fixAspNetCoreDate(c.stuffingDate, false);
    c.incomingStocks.forEach(s => {
      s.stuffingDate = fixAspNetCoreDate(s.stuffingDate, false);
    });
  });

  const newState = _.cloneDeep(state);
  newState.vessel.current = vessel;
  return newState;
}

export function vesselListAction(state: IState, list: IVesselListItem[]) {
  const newState = _.cloneDeep(state);
  newState.vessel.list = list;
  return newState;
}

export function vesselNotLoadedContainersAction(state: IState, notLoadedContainers: INotLoadedContainer[]) {
  const newState = _.cloneDeep(state);
  newState.vessel.notLoadedContainers = notLoadedContainers;
  return newState;
}
