import { IPaymentDto } from "interfaces/payments/IPaymentDto";
import { HttpClient } from "aurelia-fetch-client";
import { autoinject } from "aurelia-framework";
import { Router } from "aurelia-router";
import { Store } from "aurelia-store";
import { IPayment } from "interfaces/payments/IPayment";
import { IPaymentListItem } from "interfaces/payments/IPaymentListItem";
import _ from "lodash";
import { QueryId } from "models/QueryId";
import { IState } from "store/state";
import { fixAspNetCoreDate } from "./../core/helpers";
import { FetchService } from "./fetch-service";
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
    store.registerAction("loadPaymentsPurchasesListAction", loadPaymentsPurchasesListAction);
  }

  public async createPayment() {
    return super.get<IPayment>([this.currentCompanyIdQuery()], "create", paymentEditAction);
  }

  public async load(id: string) {
    return super.get<IPayment>(id, "load", paymentEditAction);
  }

  public async save(payment: IPayment) {
    return super.post<IPayment>(payment, "save", paymentEditAction);
  }

  public async loadList(supplierId: string) {

    return super.getMany<IPaymentListItem[]>(
      [
        this.currentCompanyIdQuery(),
        new QueryId("supplierId", supplierId)
      ], "loadList", paymentListAction);
  }

  public async loadPaymentsPurchasesList(supplierId: string) {
    return super.get<IPaymentDto>([this.currentCompanyIdQuery(), new QueryId("supplierId", supplierId)], "loadPaymentsPurchasesList", loadPaymentsPurchasesListAction);
  }

  public async deletePayment(id: string) {
    return super.delete(id, "deletePayment", noOpAction);
  }
}

export function paymentEditAction(state: IState, payment: IPayment) {
  payment.paymentDate = fixAspNetCoreDate(payment.paymentDate, false);

  const newState = _.cloneDeep(state);
  
  newState.payment.current = payment;
  newState.payment.paymentDto.paymentList=[];
  newState.payment.paymentDto .purchaseList=[];
  
  return newState;
}

export function loadPaymentsPurchasesListAction(state: IState, paymentDto: IPaymentDto) {
  const newState = _.cloneDeep(state);
  newState.payment.paymentDto = paymentDto;
  return newState;
}

export function paymentListAction(state: IState, list: IPaymentListItem[]) {
  const newState = _.cloneDeep(state);
  newState.payment.list = list;
  return newState;
}
