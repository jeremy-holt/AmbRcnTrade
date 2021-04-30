import { Router } from "aurelia-router";
import { encodeParams } from "./../../core/helpers";
import { IPaymentListItem } from "interfaces/payments/IPaymentListItem";
import { ICustomerListItem } from "core/interfaces/ICustomerListItem";
import { PaymentService } from "./../../services/payment-service";
import { CustomerService } from "./../../services/customer-service";
import { IState } from "./../../store/state";
import { autoinject, observable } from "aurelia-framework";
import { connectTo } from "aurelia-store";
import _ from "lodash";

@autoinject
@connectTo()
export class PaymentsList {
  @observable public state: IState
  protected customersList: ICustomerListItem[] = [];
  protected list: IPaymentListItem[] = [];

  @observable public selectedSupplier: ICustomerListItem = undefined!;

  constructor(
    private customerService: CustomerService,
    private paymentsService: PaymentService,
    private router: Router
  ) { }

  protected async activate() {
    await this.customerService.loadCustomersForAppUserList();
    await this.paymentsService.loadList(null);
  }

  protected stateChanged(state: IState) {
    this.customersList = _.cloneDeep(state.userFilteredCustomers);
    this.customersList.unshift({ id: null, name: "[All]" } as ICustomerListItem);
    this.list = state.payment.list;
  }

  protected async selectedSupplierChanged() {
    await this.runQuery();
  }

  protected encode(value: string) {
    return encodeParams(value);
  }

  protected addPayment() {
    this.router.navigateToRoute("paymentEdit", { id: null });
  }

  private async runQuery() {
    await this.paymentsService.loadList(this.selectedSupplier.id);
  }
}
