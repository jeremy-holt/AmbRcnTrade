import { IRemoveInspectionFromStockRequest } from "./../interfaces/stockManagement/IRemoveInspectionFromStockRequest";
import { IMoveInspectionToStockRequest } from "./../interfaces/stockManagement/IMoveInspectionToStockRequest";
import { HttpClient } from "aurelia-fetch-client";
import { FetchService } from "./fetch-service";
import { autoinject } from "aurelia-framework";
import { Router } from "aurelia-router";
import { Store } from "aurelia-store";
import { IState } from "store/state";
import { noOpAction } from "./no-op-action";

@autoinject
export class StockManagementService extends FetchService {
  constructor(
    http: HttpClient,
    store: Store<IState>,
    router: Router
  ) {
    super("api/stockManagement", http, store, router);
  }

  public async moveInspectionToStock(request: IMoveInspectionToStockRequest) {
    return super.post(request, "moveInspectionToStock", noOpAction);
  }

  public async removeInspectionFromStock(request: IRemoveInspectionFromStockRequest) {
    return super.post(request, "removeInspectionFromStock", noOpAction);
  }
}
