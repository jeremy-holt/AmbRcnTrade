import { HttpClient } from "aurelia-fetch-client";
import { autoinject } from "aurelia-framework";
import { Router } from "aurelia-router";
import { Store } from "aurelia-store";
import _ from "lodash";
import { QueryId } from "models/QueryId";
import { IState } from "store/state";
import { fixAspNetCoreDate } from "./../core/helpers";
import { IPurchase } from "./../interfaces/purchases/IPurchase";
import { IPurchaseListItem } from "./../interfaces/purchases/IPurchaseListItem";
import { IStockListItem } from "./../interfaces/stocks/IStockListItem";
import { FetchService } from "./fetch-service";

@autoinject
export class PurchaseService extends FetchService {
  constructor(
    http: HttpClient,
    store: Store<IState>,
    router: Router
  ) {
    super("api/purchases", http, store, router);

    store.registerAction("purchaseEditAction", purchaseEditAction);
    store.registerAction("purchaseListAction", purchaseListAction);
  }

  public async load(id: string) {
    return await super.get<IPurchase>(id, "load", purchaseEditAction);
  }

  public async save(purchase: IPurchase) {
    return await super.post<IPurchase>(purchase, "save", purchaseEditAction);
  }

  public async loadList(supplierId: string) {
    return await super.getMany<IPurchaseListItem[]>(
      [
        this.currentCompanyIdQuery(),
        new QueryId("supplierId", supplierId)
      ], "loadList", purchaseListAction);
  }

  public async createPurchase() {
    return await super.get([super.currentCompanyIdQuery()], "create", purchaseEditAction);
  }

  public getStockAverages(stocks: IStockListItem[]) {
    const bags = stocks.reduce((a, b) => a += b.bagsIn, 0);
    const kor = bags > 0 ? stocks.reduce((a, b) => a += (b.bagsIn * b.analysisResult.kor), 0) / bags : 0;
    const count = bags > 0 ? stocks.reduce((a, b) => a += (b.bagsIn * b.analysisResult.count), 0) / bags : 0;
    const moisture = bags > 0 ? stocks.reduce((a, b) => a += (b.bagsIn * b.analysisResult.moisture), 0) / bags : 0;
    const weightKg = stocks.reduce((a, b) => a += b.weightKgIn, 0);

    return {
      bags,
      kor,
      count,
      moisture,
      weightKg
    };
  }
}

export function purchaseEditAction(state: IState, purchase: IPurchase) {
  purchase.purchaseDate = fixAspNetCoreDate(purchase.purchaseDate, false);
  purchase.deliveryDate = fixAspNetCoreDate(purchase.deliveryDate, false);
  purchase.purchaseDetails.forEach(c => {
    c.priceAgreedDate = fixAspNetCoreDate(c.priceAgreedDate, false);
  });

  const newState = _.cloneDeep(state);
  newState.purchase.current = purchase;
  return newState;
}

export function purchaseListAction(state: IState, list: IPurchaseListItem[]) {
  const newState = _.cloneDeep(state);
  newState.purchase.list = list;
  return newState;
}
