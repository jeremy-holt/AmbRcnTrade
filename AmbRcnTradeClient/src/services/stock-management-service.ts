import { IUnAllocatedStock } from "./../interfaces/stockManagement/IUnallocatedStock";
import { HttpClient } from "aurelia-fetch-client";
import { autoinject } from "aurelia-framework";
import { Router } from "aurelia-router";
import { Store } from "aurelia-store";
import { IStock } from "interfaces/stocks/IStock";
import _ from "lodash";
import { IState } from "store/state";
import { IMovedInspectionResult } from "./../interfaces/stockManagement/IMovedInspectionResult";
import { IMoveInspectionToStockRequest } from "./../interfaces/stockManagement/IMoveInspectionToStockRequest";
import { IRemoveInspectionFromStockRequest } from "./../interfaces/stockManagement/IRemoveInspectionFromStockRequest";
import { FetchService } from "./fetch-service";
import { noOpAction } from "./no-op-action";

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
    store.registerAction("nonCommittedStocksListAction", nonCommittedStocksListAction);
  }

  public async moveInspectionToStock(request: IMoveInspectionToStockRequest) {
    await this.store.dispatch(inspectionMoveToStockClearAction);

    return await super.post<string>(request, "moveInspectionToStock", inspectionMoveToStockAction);
  }

  public async removeInspectionFromStock(request: IRemoveInspectionFromStockRequest) {
    return super.post(request, "removeInspectionFromStock", noOpAction);
  }

  public async getNonCommittedStocks() {
    return super.getMany<IStock[]>([super.currentCompanyIdQuery()], "getNonCommittedStocks", nonCommittedStocksListAction);
  }
}

export function inspectionMoveToStockAction(state: IState, result: IMovedInspectionResult) {
  const newState = _.cloneDeep(state);
  newState.inspection.movedToStockId = result.stockId;
  newState.inspection.current = result.inspection;
  return newState;
}

export function nonCommittedStocksListAction(state: IState, stocks: IUnAllocatedStock[]) {
  const newState = _.cloneDeep(state);
  newState.purchase.nonCommittedStocksList = stocks;
  return newState;
}

function inspectionMoveToStockClearAction(state: IState) {
  const newState = _.cloneDeep(state);
  newState.inspection.movedToStockId = undefined!;
  return newState;
}
