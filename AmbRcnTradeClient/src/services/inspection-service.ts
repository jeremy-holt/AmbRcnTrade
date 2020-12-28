import { fixAspNetCoreDate } from "./../core/helpers";
import { Approval } from "./../constants/app-constants";
import { IInspectionListItem } from "./../interfaces/inspections/IInspectionListItem";
import { IInspection } from "./../interfaces/inspections/IInspection";
import { Router } from "aurelia-router";
import { HttpClient } from "aurelia-fetch-client";
import { autoinject } from "aurelia-framework";
import { FetchService } from "./fetch-service";
import { IState } from "store/state";
import { Store } from "aurelia-store";
import _ from "lodash";
import { QueryId } from "models/QueryId";

@autoinject
export class InspectionService extends FetchService {
  constructor(
    http: HttpClient,
    store: Store<IState>,
    router: Router
  ) {
    super("api/inspection", http, store, router);

    store.registerAction("inspectionEditAction", inspectionEditAction);
    store.registerAction("inspectionListAction", inspectionListAction);
  }

  public async load(id: string) {
    return super.get(id, "load", inspectionEditAction);
  }

  public async save(model: IInspection) {
    model.inspectionDate = fixAspNetCoreDate(model.inspectionDate, false);

    return super.post(model, "save", inspectionEditAction);
  }

  public async loadList(approval: Approval) {
    return super.getMany<IInspectionListItem[]>([super.currentCompanyIdQuery(), new QueryId("approval", approval)], "loadList", inspectionListAction);
  }
}

export function inspectionEditAction(state: IState, inspection: IInspection) {
  const newState = _.cloneDeep(state);
  newState.inspection.current = inspection;
  return newState;
}

export function inspectionListAction(state: IState, list: IInspectionListItem[]) {
  const newState = _.cloneDeep(state);
  newState.inspection.list = list;
  return newState;
}
