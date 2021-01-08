import { IPaymentListItem } from "interfaces/payments/IPaymentListItem";
import { ICustomerListItem } from "./../../interfaces/ICustomerListItem";
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
    private paymentsService: PaymentService
  ) { }

  protected async activate() {
    await this.customerService.loadCustomersForAppUserList();
  }

  protected stateChanged(state: IState) {
    this.customersList = _.cloneDeep(state.userFilteredCustomers);
    this.customersList.unshift({ id: null, name: "[All]" } as ICustomerListItem);
    this.list = state.paymentDto.list;
  }

  protected async selectedSupplierChanged(value: ICustomerListItem) {
    if (value?.id) {
      await this.runQuery();
    }
  }


  private async runQuery() {
    await this.paymentsService.loadList(this.selectedSupplier.id);
  }
}
