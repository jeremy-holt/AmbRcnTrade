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
    purchase.purchaseDate = fixAspNetCoreDate(purchase.purchaseDate, false);
    purchase.purchaseDetails.forEach(c => {
      c.date = fixAspNetCoreDate(c.date, false);
    });

    return super.post<IPurchase>(purchase, "save", purchaseEditAction);
  }

  public async loadList() {
    return super.getMany<IPurchaseListItem[]>([this.currentCompanyIdQuery()], "loadList", purchaseListAction);
  }
}

export function purchaseEditAction(state: IState, purchase: IPurchase) {
  const newState = _.cloneDeep(state);
  newState.purchase.current = purchase;
  return newState;
}

export function purchaseListAction(state: IState, list: IPurchaseListItem[]) {
  const newState = _.cloneDeep(state);
  newState.purchase.list = list;
  return newState;
}
