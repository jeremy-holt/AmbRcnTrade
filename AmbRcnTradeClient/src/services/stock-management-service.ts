import { IBlendStockRequest } from "./../interfaces/stockManagement/IBlendStockRequest";
import { HttpClient } from "aurelia-fetch-client";
import { autoinject } from "aurelia-framework";
import { Router } from "aurelia-router";
import { Store } from "aurelia-store";
import { ContainerStatus } from "constants/app-constants";
import { IStuffingRequest } from "interfaces/stockManagement/IStuffingRequest";
import { IStock } from "interfaces/stocks/IStock";
import _ from "lodash";
import { QueryId } from "core/services/QueryId";
import { IState } from "store/state";
import { IAvailableContainer } from "./../interfaces/stockManagement/IAvailableContainerItem";
import { IOutgoingStock } from "./../interfaces/stockManagement/IIncomingStock";
import { IMovedInspectionResult } from "./../interfaces/stockManagement/IMovedInspectionResult";
import { IMoveInspectionToStockRequest } from "./../interfaces/stockManagement/IMoveInspectionToStockRequest";
import { IRemoveInspectionFromStockRequest } from "./../interfaces/stockManagement/IRemoveInspectionFromStockRequest";
import { IStockBalance } from "./../interfaces/stocks/IStockBalance";
import { IStockListItem } from "./../interfaces/stocks/IStockListItem";
import { FetchService } from "core/services/fetch-service";
import { noOpAction } from "core/services/no-op-action";
import { IBlendedStock } from "interfaces/stockManagement/IBlendedStock";

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
    store.registerAction("blendStockAction", blendStockAction);
  }

  public async moveInspectionToStock(request: IMoveInspectionToStockRequest) {
    await this.store.dispatch(inspectionMoveToStockClearAction);

    return await super.post<string>(request, "moveInspectionToStock", inspectionMoveToStockAction);
  }

  public async removeInspectionFromStock(request: IRemoveInspectionFromStockRequest) {
    return await super.post(request, "removeInspectionFromStock", noOpAction);
  }

  public async getNonCommittedStocks(supplierId: string) {
    return await super.getMany<IStock[]>([super.currentCompanyIdQuery(), new QueryId("supplierId", supplierId)], "getNonCommittedStocks", nonCommittedStocksListAction);
  }

  public async stuffContainer(containerId: string, stuffingDate: string, stockBalance: IStockBalance, bags: number, weightKg: number, status: ContainerStatus) {
    const request: IStuffingRequest = {
      containerId,
      stuffingDate,
      stockBalance,
      bags,
      weightKg,
      status
    };
    return await super.post(request, "stuffContainer", stuffContainerAction);
  }

  public async blendStock(stockBalance: IStockBalance, bagsToBlend: number, lotNo: number, blendingDate: string) {
    const request: IBlendStockRequest = { stockBalance, bagsToBlend, lotNo, blendingDate };
    return await super.post(request, "blendStock", blendStockAction);
  }

  public async getAvailableContainers() {
    return await super.get<IAvailableContainer[]>([super.currentCompanyIdQuery()], "getAvailableContainers", availableContainersAction);
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

export function blendStockAction(state: IState, result: IBlendedStock) {
  const newState = _.cloneDeep(state);
  newState.stockManagement.blendedStock = result;
  return newState;
}

export function availableContainersAction(state: IState, result: IAvailableContainer[]) {
  const newState = _.cloneDeep(state);
  newState.stockManagement.availableContainers = result;
  return newState;
}

function inspectionMoveToStockClearAction(state: IState) {
  const newState = _.cloneDeep(state);
  newState.inspection.movedToStockId = undefined!;
  return newState;
}
