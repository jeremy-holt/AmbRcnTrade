import { HttpClient } from "aurelia-fetch-client";
import { autoinject } from "aurelia-framework";
import { Router } from "aurelia-router";
import { Store } from "aurelia-store";
import { IVessel } from "interfaces/shipping/IVessel";
import _ from "lodash";
import { IState } from "store/state";
import { fixAspNetCoreDate } from "./../core/helpers";
import { IVesselContainersRequest } from "./../interfaces/shipping/IVesselContainerRequest";
import { IVesselListItem } from "./../interfaces/shipping/IVesselListItem";
import { FetchService } from "./fetch-service";
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

  public async addBillLadingToVessel(request: IVesselContainersRequest) {
    return super.post(request, "addBillLadingToVessel", noOpAction);
  }

  public async removeBillsLadingFromVessel(request: IVesselContainersRequest) {
    return super.post(request, "removeBillsLadingFromVessel", noOpAction);
  }

  public async createVessel() {
    return super.get<IVessel>([this.currentCompanyIdQuery()], "create", vesselEditFunction);
  }

}

export function vesselEditFunction(state: IState, vessel: IVessel) {
  vessel.etaHistory.forEach(c => {
    c.dateUpdated = fixAspNetCoreDate(c.dateUpdated, false);
    c.eta = fixAspNetCoreDate(c.eta, false);
  });

  vessel.billLadings.forEach(c => {
    c.blDate = fixAspNetCoreDate(c.blDate, false);
    c.containers.forEach(x => {
      x.dispatchDate = fixAspNetCoreDate(x.dispatchDate, false),
      x.incomingStocks.forEach(s => {
        s.stuffingDate = fixAspNetCoreDate(s.stuffingDate, false);
      });
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
