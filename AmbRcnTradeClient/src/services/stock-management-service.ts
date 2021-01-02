import { DATEFORMAT } from "constants/app-constants";
import { IStockBalanceListItem } from "./../interfaces/stocks/IStockBalanceListItem";
import { IAvailableContainerItem } from "./../interfaces/stockManagement/IAvailableContainerItem";
import { IOutgoingStock } from "./../interfaces/stockManagement/IIncomingStock";
import { IStockListItem } from "./../interfaces/stocks/IStockListItem";
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
import { QueryId } from "models/QueryId";
import { IStuffingRequest } from "interfaces/stockManagement/IStuffingRequest";
import moment from "moment";

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
    store.registerAction("stuffContainerAction", stuffContainerAction);
    store.registerAction("availableContainersAction", availableContainersAction);
  }

  public async moveInspectionToStock(request: IMoveInspectionToStockRequest) {
    await this.store.dispatch(inspectionMoveToStockClearAction);

    return await super.post<string>(request, "moveInspectionToStock", inspectionMoveToStockAction);
  }

  public async removeInspectionFromStock(request: IRemoveInspectionFromStockRequest) {
    return super.post(request, "removeInspectionFromStock", noOpAction);
  }

  public async getNonCommittedStocks(supplierId: string) {
    return super.getMany<IStock[]>([super.currentCompanyIdQuery(), new QueryId("supplierId", supplierId)], "getNonCommittedStocks", nonCommittedStocksListAction);
  }

  public async stuffContainer(request: IStuffingRequest) {
    return super.post(request, "stuffContainer", stuffContainerAction);
  }

  public async getAvailableContainers() {
    return super.get([super.currentCompanyIdQuery()], "getAvailableContainers", availableContainersAction);
  }

  public getStuffingRequest(stockId: string, containerId: string, bags: number, weightKg: number): IStuffingRequest {
    return {
      containerId,
      stuffingDate: moment().format(DATEFORMAT),
      incomingStocks: [
        {
          stockId,
          bags,
          weightKg
        }
      ]
    };
  }
}

export function inspectionMoveToStockAction(state: IState, result: IMovedInspectionResult) {
  const newState = _.cloneDeep(state);
  newState.inspection.movedToStockId = result.stockId;
  newState.inspection.current = result.inspection;
  return newState;
}

export function nonCommittedStocksListAction(state: IState, stocks: IStockListItem[]) {
  const newState = _.cloneDeep(state);
  newState.purchase.nonCommittedStocksList = stocks;
  return newState;
}

export function stuffContainerAction(state: IState, result: IOutgoingStock[]) {
  const newState = _.cloneDeep(state);
  newState.stockManagement.stuffContainer = result;
  return newState;
}

export function availableContainersAction(state: IState, result: IAvailableContainerItem[]) {
  const newState = _.cloneDeep(state);
  newState.stockManagement.availableContainers = result;
  return newState;
}

function inspectionMoveToStockClearAction(state: IState) {
  const newState = _.cloneDeep(state);
  newState.inspection.movedToStockId = undefined!;
  return newState;
}
