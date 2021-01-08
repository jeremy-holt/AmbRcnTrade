import { CURRENCIES_LIST, Currency } from "./../../constants/app-constants";
import { ICustomerListItem } from "./../../interfaces/ICustomerListItem";
import { IParamsId } from "./../../interfaces/IParamsId";
import { PaymentService } from "./../../services/payment-service";
import { CustomerService } from "./../../services/customer-service";
import { IPaymentDto } from "./../../interfaces/payments/IPaymentDto";
import { IState } from "./../../store/state";
import { autoinject, observable } from "aurelia-framework";
import { connectTo } from "aurelia-store";
import _ from "lodash";

@autoinject
@connectTo()
export class PaymentEdit {
  @observable protected state: IState = undefined!;

  protected customerList: ICustomerListItem[] = [];
  protected model: IPaymentDto = undefined;
  protected currencyList = CURRENCIES_LIST;

  constructor(
    private customerService: CustomerService,
    private paymentService: PaymentService
  ) { }

  protected async activate(prms: IParamsId) {
    await this.customerService.loadCustomersForAppUserList();

    if (prms?.id) {
      await this.paymentService.load(prms.id);
    } else {
      await this.paymentService.createPayment();
    }
  }

  protected stateChanged(state: IState) {
    this.customerList = _.cloneDeep(state.userFilteredCustomers);
    this.customerList.unshift({ id: null, name: "[Select]" } as ICustomerListItem);

    this.model = state.paymentDto.current;
    if (!this.model?.payment?.id) {
      this.model.payment.currency = this.currencyList.find(c => c.id === Currency.CFA)?.id;
    }
  }

  protected get caption() {
    return this.model?.payment.id ? `Payment no ${this.model.payment.paymentNo}` : "New payment";
  }

  protected get canSave() {
    return this.model?.payment.paymentDate && this.model.payment.value > 0 && this.model.payment.supplierId !== null && this.needExchangeRate;
  }

  protected get needExchangeRate(){
    return this.model?.payment.currency !== Currency.USD && this.model?.payment.value > 0;
  }

  protected async save() {
    await this.paymentService.save(this.model);
  }
}
