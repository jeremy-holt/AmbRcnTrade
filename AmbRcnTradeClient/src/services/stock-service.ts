import { fixAspNetCoreDate } from "./../core/helpers";
import { IStockBalance } from "./../interfaces/stocks/IStockBalance";
import { IStockListItem } from "./../interfaces/stocks/IStockListItem";
import { IStock } from "interfaces/stocks/IStock";
import { Router } from "aurelia-router";
import { HttpClient } from "aurelia-fetch-client";
import { FetchService } from "core/services/fetch-service";
import { autoinject } from "aurelia-framework";
import { Store } from "aurelia-store";
import { IState } from "store/state";
import _ from "lodash";
import { QueryId } from "core/services/QueryId";
import { noOpAction } from "core/services/no-op-action";

@autoinject
export class StockService extends FetchService {
  constructor(
    http: HttpClient,
    store: Store<IState>,
    router: Router
  ) {
    super("api/stocks", http, store, router);

    store.registerAction("stockEditAction", stockEditAction);
    store.registerAction("stockListAction", stockListAction);
    store.registerAction("stockListBalanceAction", stockBalanceListAction);
  }

  public async load(id: string) {
    return await super.get(id, "load", stockEditAction);
  }

  public async save(stock: IStock) {
    return await super.post<IStock>(stock, "save", stockEditAction);
  }

  public async loadStockList(locationId: string) {    
    return await super.getMany<IStockListItem[]>(
      [
        this.currentCompanyIdQuery(),
        new QueryId("locationId", locationId)
      ], "loadStockList", stockListAction
    );
  }

  public async loadStockBalanceList(locationId: string) {
    return await super.getMany<IStockListItem[]>(
      [
        this.currentCompanyIdQuery(),
        new QueryId("locationId", locationId)
      ], "loadStockBalanceList", stockBalanceListAction
    );
  }

  public async createStock() {
    return await super.get([super.currentCompanyIdQuery()], "create", stockEditAction);
  }

  public async deleteStock(id: string){
    return await super.delete(id,"delete",noOpAction);
  }
}


export function stockEditAction(state: IState, stock: IStock) {
  stock.stockInDate = fixAspNetCoreDate(stock.stockInDate, false);
  stock.stockOutDate = fixAspNetCoreDate(stock.stockOutDate, false);

  const newState = _.cloneDeep(state);
  newState.stock.current = stock;
  return newState;
}

export function stockListAction(state: IState, list: IStockListItem[]) {
  const newState = _.cloneDeep(state);
  newState.stock.list = list;
  return newState;
}

export function stockBalanceListAction(state: IState, list: IStockBalance[]) {
  const newState = _.cloneDeep(state);
  newState.stock.stockBalanceList = list;
  return newState;
}
