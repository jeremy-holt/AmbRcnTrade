import { Router } from "aurelia-router";
import { HttpClient } from "aurelia-fetch-client";
import { IState } from "store/state";
import { FetchService } from "./fetch-service";
import _ from "lodash";
import { IPaymentListItem } from "interfaces/payments/IPaymentListItem";
import { autoinject } from "aurelia-framework";
import { Store } from "aurelia-store";
import { IPaymentDto } from "interfaces/payments/IPaymentDto";
import { QueryId } from "models/QueryId";
import { noOpAction } from "./no-op-action";

@autoinject
export class PaymentService extends FetchService {
  constructor(
    http: HttpClient,
    store: Store<IState>,
    router: Router
  ) {
    super("api/payments", http, store, router);

    store.registerAction("paymentEditAction", paymentEditAction);
    store.registerAction("paymentListAction", paymentListAction);
  }

  public async createPayment() {
    return super.get<IPaymentDto>([this.currentCompanyIdQuery()], "create", paymentEditAction);
  }

  public async load(id: string) {
    return super.get<IPaymentDto>(id, "load", paymentEditAction);
  }

  public async save(paymentDto: IPaymentDto) {
    return super.post<IPaymentDto>(paymentDto, "save", paymentEditAction);
  }

  public async loadList(supplierId: string) {

    return super.getMany<IPaymentListItem[]>(
      [
        this.currentCompanyIdQuery(),
        new QueryId("supplierId", supplierId)        
      ], "loadList", paymentListAction);
  }

  public async deletePayment(id: string) {
    return super.delete(id, "deletePayment", noOpAction);
  }
}

export function paymentEditAction(state: IState, dto: IPaymentDto) {
  const newState = _.cloneDeep(state);
  newState.paymentDto.current = dto;
  return newState;
}

export function paymentListAction(state: IState, list: IPaymentListItem[]) {
  const newState = _.cloneDeep(state);
  newState.paymentDto.list = list;
  return newState;
}
