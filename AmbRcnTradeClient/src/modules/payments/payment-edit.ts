import { Router } from "aurelia-router";
import { DeleteDialog } from "./../../dialogs/delete-dialog";
import { DialogService } from "aurelia-dialog";
import { encodeParams } from "./../../core/helpers";
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
import { IPayment } from "interfaces/payments/IPayment";
import numbro from "numbro";

@autoinject
@connectTo()
export class PaymentEdit {
  @observable protected state: IState = undefined!;

  protected customerList: ICustomerListItem[] = [];
  protected model: IPayment = undefined;
  protected paymentDto: IPaymentDto = undefined!;
  protected currencyList = CURRENCIES_LIST;
  @observable protected selectedSupplier: ICustomerListItem = undefined!;
  protected valueUsd: number = undefined;

  constructor(
    private customerService: CustomerService,
    private paymentService: PaymentService,
    private dialogService: DialogService,
    private router: Router
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

    this.model = _.cloneDeep(state.payment.current);

    if (!this.model?.id) {
      this.model.currency = this.currencyList.find(c => c.id === Currency.CFA)?.id;
    }

    this.calcValue();
    this.paymentDto = state.payment.paymentDto;
  }

  protected async selectedSupplierChanged(value: ICustomerListItem) {
    if (value.id) {
      await this.paymentService.loadPaymentsPurchasesList(value?.id);
    }
  }

  protected bind() {
    this.selectedSupplier = this.customerList.find(c => c.id === this.model?.supplierId);
  }

  protected get caption() {
    return this.model?.id ? `Payment no ${numbro(this.model.paymentNo).format("00000")}` : "New payment";
  }

  protected get canSave() {
    if (this.needExchangeRate && this.model?.exchangeRate === 0) {
      return false;
    }
    return this.model?.paymentDate && this.model.value > 0 && this.selectedSupplier?.id !== null;
  }

  protected get needExchangeRate() {
    return this.model?.currency !== Currency.USD;
  }

  protected async save() {
    this.model.supplierId = this.selectedSupplier?.id;
    if (this.model.currency === Currency.USD) {
      this.model.exchangeRate = 1;
    }

    await this.paymentService.save(this.model);
    await this.paymentService.loadPaymentsPurchasesList(this.model.supplierId);
  }

  protected calcValue() {
    if (this.model.currency !== Currency.USD && this.model.exchangeRate > 0) {
      this.valueUsd = +this.model.value / +this.model.exchangeRate;
    }
  }

  protected get showNeedExchangeRateMessage() {
    return this.needExchangeRate && this.model?.exchangeRate === 0;
  }

  protected encode(value: string) {
    return encodeParams(value);
  }

  protected async deletePayment() {
    this.dialogService.open({ viewModel: DeleteDialog }).whenClosed(async result => {
      if (!result.wasCancelled) {
        await this.paymentService.deletePayment(this.model.id);
        this.router.navigateToRoute("paymentList");
      }
    });
  }
}