
import { DialogService } from "aurelia-dialog";
import { autoinject, observable, TaskQueue } from "aurelia-framework";
import { connectTo } from "aurelia-store";
import _ from "lodash";
import { StockManagementService } from "services/stock-management-service";
import { IState } from "store/state";
import { CURRENCIES_LIST, Currency } from "./../../constants/app-constants";
import { DeleteDialog } from "./../../dialogs/delete-dialog";
import { IListItem } from "./../../interfaces/IEntity";
import { IParamsId } from "./../../interfaces/IParamsId";
import { IPurchase } from "./../../interfaces/purchases/IPurchase";
import { IPurchaseDetail } from "./../../interfaces/purchases/IPurchaseDetail";
import { IStockListItem } from "./../../interfaces/stocks/IStockListItem";
import { CustomerService } from "./../../services/customer-service";
import { PurchaseService } from "./../../services/purchase-service";
import { UncommittedStocksDialog } from "./uncommitted-stocks-dialog";

@autoinject
@connectTo()
export class PurchaseEdit {
  @observable protected state: IState = undefined!;
  protected model: IPurchase = undefined!;
  protected uncommittedStocks: IStockListItem[] = [];

  protected currencies = CURRENCIES_LIST;

  protected suppliers: IListItem[] = [];
  @observable selectedSupplier: IListItem = undefined!;

  constructor(
    private purchaseService: PurchaseService,
    private customerService: CustomerService,
    private dialogService: DialogService,
    private stockManagementService: StockManagementService,
    private taskQueue: TaskQueue
  ) {
  }

  protected stateChanged(state: IState) {
    this.suppliers = _.cloneDeep(state.userFilteredCustomers);
    this.suppliers.unshift({ id: null, name: "[Select]" });
    this.model = _.cloneDeep(state.purchase.current);

    if (this.model) {
      this.model.purchaseDetails.forEach(detail => {
        detail.values = this.purchaseService.getStockAverages(detail.stocks);
      });
    }
    this.uncommittedStocks = _.cloneDeep(state.purchase.nonCommittedStocksList);
  }

  protected async activate(params: IParamsId) {
    await this.customerService.loadCustomersForAppUserList();

    if (params?.id) {
      await this.purchaseService.load(params.id);
    } else {
      await this.purchaseService.createPurchase();
    }
  }

  protected bind() {
    this.selectedSupplier = this.suppliers.find(c => c.id === this.model?.supplierId);
  }

  protected addDetail() {
    this.model.purchaseDetails.push({
      priceAgreedDate: this.model.purchaseDate,
      currency: CURRENCIES_LIST.find(c => c.id === Currency.CFA).id,
      pricePerKg: undefined!,
      exchangeRate: undefined!,
      stocks: [],
      value: undefined!,
      valueUsd: undefined!
    });
  }

  protected addStocks(index: number) {
    this.dialogService.open(
      {
        viewModel: UncommittedStocksDialog,
        model: { uncommittedStocks: this.uncommittedStocks, supplier: this.selectedSupplier }
      }
    ).whenClosed(result => {
      if (!result.wasCancelled) {
        const detail = this.model.purchaseDetails[index];
        const stocks = result.output as IStockListItem[];

        detail.stockIds = stocks.map(x => x.stockId);
        detail.stocks = stocks;
        detail.values = this.purchaseService.getStockAverages(stocks);
      }
    });
  }

  protected deleteStockDetail(detail: IPurchaseDetail, index: number) {
    detail.stockIds.splice(index, 1);
    detail.stocks.splice(index);
  }

  protected async selectedSupplierChanged(value: IListItem) {
    if (value.id) {
      await this.stockManagementService.getNonCommittedStocks(value.id);
    }
  }

  protected deletePurchaseDetail(index: number) {
    this.dialogService.open({
      viewModel: DeleteDialog
    }).whenClosed(result => {
      if (!result.wasCancelled) {
        this.model.purchaseDetails.splice(index, 1);
      }
    });
  }

  protected get canSave() {
    if (!this.model) {
      return false;
    }

    const header = this.selectedSupplier?.id && this.model.purchaseDate && this.model.quantityMt > 0;
    const details = this.model?.purchaseDetails?.length > 0 && this.model.purchaseDetails.every(detail => detail.pricePerKg > 0 && detail?.stockIds?.length > 0);

    if (this.model?.purchaseDetails.length === 0 && header) {
      return true;
    }

    return header && details;
  }

  protected async save() {
    if (this.canSave) {
      this.model.supplierId = this.selectedSupplier.id;
      await this.purchaseService.save(this.model);
    }
  }
}
