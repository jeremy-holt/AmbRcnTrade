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
import { FetchService } from "core/services/fetch-service";
import { noOpAction } from "core/services/no-op-action";

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
    return await super.post<IVessel>(vessel, "save", vesselEditFunction);
  }

  public async load(id: string) {
    return await super.get<IVessel>(id, "load", vesselEditFunction);
  }

  public async loadList() {
    return await super.getMany<IVesselListItem[]>([this.currentCompanyIdQuery()], "loadList", vesselListAction);
  }

  public async addBillLadingToVessel(request: IVesselContainersRequest) {
    return await super.post(request, "addBillLadingToVessel", noOpAction);
  }

  public async createVessel() {
    return await super.get<IVessel>([this.currentCompanyIdQuery()], "create", vesselEditFunction);
  }

  public async deleteVessel(id: string) {
    return await super.delete(id, "deleteVessel", noOpAction);
  }

}

export function vesselEditFunction(state: IState, vessel: IVessel) {
  vessel.eta = fixAspNetCoreDate(vessel.eta, false);

  vessel.billLadings.forEach(c => {
    c.blDate = fixAspNetCoreDate(c.blDate, false);
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
