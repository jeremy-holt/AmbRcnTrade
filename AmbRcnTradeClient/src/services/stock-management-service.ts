import { IMovedInspectionResult } from "./../interfaces/stockManagement/IMovedInspectionResult";
import { IRemoveInspectionFromStockRequest } from "./../interfaces/stockManagement/IRemoveInspectionFromStockRequest";
import { IMoveInspectionToStockRequest } from "./../interfaces/stockManagement/IMoveInspectionToStockRequest";
import { HttpClient } from "aurelia-fetch-client";
import { FetchService } from "./fetch-service";
import { autoinject } from "aurelia-framework";
import { Router } from "aurelia-router";
import { Store } from "aurelia-store";
import { IState } from "store/state";
import { noOpAction } from "./no-op-action";
import _ from "lodash";

@autoinject
export class StockManagementService extends FetchService {
  constructor(
    http: HttpClient,
    store: Store<IState>,
    router: Router
  ) {
    super("api/stockManagement", http, store, router);

    store.registerAction("inspectionMoveToStockAction", inspectionMoveToStockAction);
    store.registerAction("inspectionMoveToStockClearAction", inspectionMoveToStockClearAction);
  }

  public async moveInspectionToStock(request: IMoveInspectionToStockRequest) {
    await this.store.dispatch(inspectionMoveToStockClearAction);

    return await super.post<string>(request, "moveInspectionToStock", inspectionMoveToStockAction);
  }

  public async removeInspectionFromStock(request: IRemoveInspectionFromStockRequest) {
    return super.post(request, "removeInspectionFromStock", noOpAction);
  }
}

export function inspectionMoveToStockAction(state: IState, result: IMovedInspectionResult) {
  const newState = _.cloneDeep(state);
  newState.inspection.movedToStockId = result.stockId;
  newState.inspection.current = result.inspection;
  return newState;
}

function inspectionMoveToStockClearAction(state: IState) {
  const newState = _.cloneDeep(state);
  newState.inspection.movedToStockId = undefined!;
  return newState;
}
