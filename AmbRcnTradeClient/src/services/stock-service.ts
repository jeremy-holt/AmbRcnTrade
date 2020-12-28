import { fixAspNetCoreDate } from "./../core/helpers";
import { IStockBalanceListItem } from "./../interfaces/stocks/IStockBalanceListItem";
import { IStockListItem } from "./../interfaces/stocks/IStockListItem";
import { IStock } from "interfaces/stocks/IStock";
import { Router } from "aurelia-router";
import { HttpClient } from "aurelia-fetch-client";
import { FetchService } from "./fetch-service";
import { autoinject } from "aurelia-framework";
import { Store } from "aurelia-store";
import { IState } from "store/state";
import _ from "lodash";
import { QueryId } from "models/QueryId";

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
    return super.get(id, "load", stockEditAction);
  }

  public async save(stock: IStock) {
    stock.stockInDate = fixAspNetCoreDate(stock.stockInDate, false);
    stock.stockOutDate = fixAspNetCoreDate(stock.stockOutDate, false);
    return super.post<IStock>(stock, "save", stockEditAction);
  }

  public async loadStockList(lotNo: number, locationId: string) {
    return super.getMany<IStockListItem[]>(
      [
        this.currentCompanyIdQuery(),
        new QueryId("lotNo", lotNo),
        new QueryId("locationId", locationId)
      ], "loadStockList", stockListAction
    );
  }

  public async loadStockBalanceList(lotNo: number, locationId: string) {
    return super.getMany<IStockListItem[]>(
      [
        this.currentCompanyIdQuery(),
        new QueryId("lotNo", lotNo),
        new QueryId("locationId", locationId)
      ], "loadStockBalanceList", stockBalanceListAction
    );
  }
}


export function stockEditAction(state: IState, stock: IStock) {
  const newState = _.cloneDeep(state);
  newState.stock.current = stock;
  return newState;
}

export function stockListAction(state: IState, list: IStockListItem[]) {
  const newState = _.cloneDeep(state);
  newState.stock.list = list;
  return newState;
}

export function stockBalanceListAction(state: IState, list: IStockBalanceListItem[]) {
  const newState = _.cloneDeep(state);
  newState.stock.stockBalanceList = list;
  return newState;
}
