import { IStock } from "interfaces/stocks/IStock";
import { fixAspNetCoreDate } from "./../core/helpers";
import { IPurchaseListItem } from "./../interfaces/purchases/IPurchaseListItem";
import { IPurchase } from "./../interfaces/purchases/IPurchase";
import { Router } from "aurelia-router";
import { HttpClient } from "aurelia-fetch-client";
import { FetchService } from "./fetch-service";
import { autoinject } from "aurelia-framework";
import { Store } from "aurelia-store";
import { IState } from "store/state";
import _ from "lodash";
import { QueryId } from "models/QueryId";

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
    return super.get<IPurchase>(id, "load", purchaseEditAction);
  }

  public async save(purchase: IPurchase) {
    return super.post<IPurchase>(purchase, "save", purchaseEditAction);
  }

  public async loadList(supplierId: string) {
    return super.getMany<IPurchaseListItem[]>(
      [
        this.currentCompanyIdQuery(),
        new QueryId("supplierId", supplierId)
      ], "loadList", purchaseListAction);
  }

  public async createPurchase() {
    return super.get([super.currentCompanyIdQuery()], "create", purchaseEditAction);
  }

  public getStockAverages(stocks: IStock[]) {
    const bags = stocks.reduce((a, b) => a += b.bags, 0);
    const kor = stocks.reduce((a, b) => a += (b.bags * b.analysisResult.kor), 0) / bags;
    const count=stocks.reduce((a, b) => a += (b.bags * b.analysisResult.count), 0) / bags;
    const moisture =stocks.reduce((a, b) => a += (b.bags * b.analysisResult.moisture), 0) / bags;
    
    return {
      bags,
      kor,
      count,
      moisture
    };
  }
}

export function purchaseEditAction(state: IState, purchase: IPurchase) {
  purchase.purchaseDate = fixAspNetCoreDate(purchase.purchaseDate, false);
  purchase.deliveryDate=fixAspNetCoreDate(purchase.deliveryDate,false);
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
