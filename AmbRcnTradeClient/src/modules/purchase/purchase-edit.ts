import { IUnAllocatedStock } from "./../../interfaces/stockManagement/IUnallocatedStock";
import { IPurchaseDetail } from "./../../interfaces/purchases/IPurchaseDetail";
import { DialogService } from "aurelia-dialog";
import { autoinject, observable } from "aurelia-framework";
import { connectTo } from "aurelia-store";
import { IStock } from "interfaces/stocks/IStock";
import _ from "lodash";
import { StockManagementService } from "services/stock-management-service";
import { IState } from "store/state";
import { CURRENCIES_LIST, Currency } from "./../../constants/app-constants";
import { IListItem } from "./../../interfaces/IEntity";
import { IParamsId } from "./../../interfaces/IParamsId";
import { IPurchase } from "./../../interfaces/purchases/IPurchase";
import { CustomerService } from "./../../services/customer-service";
import { PurchaseService } from "./../../services/purchase-service";
import { UncommittedStocksDialog } from "./uncommitted-stocks-dialog";

@autoinject
@connectTo()
export class PurchaseEdit {
  @observable protected state: IState = undefined!;
  protected model: IPurchase = undefined;
  protected uncommittedStocks: IUnAllocatedStock[] = [];

  protected currencies = CURRENCIES_LIST;

  protected suppliers: IListItem[] = [];
  @observable selectedSupplier: IListItem = undefined!;

  constructor(
    private purchaseService: PurchaseService,
    private customerService: CustomerService,
    private dialogService: DialogService,
    private stockManagementService: StockManagementService
  ) { }

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

    this.selectedSupplier = this.suppliers.find(c => c.id === this.model?.supplierId);
  }

  protected async activate(params: IParamsId) {
    await this.customerService.loadCustomersForAppUserList();
    await this.stockManagementService.getNonCommittedStocks();

    if (params?.id) {
      await this.purchaseService.load(params.id);
    } else {
      await this.purchaseService.createPurchase();
    }
  }

  protected addDetail() {
    this.model.purchaseDetails.push({
      date: this.model.purchaseDate,
      currency: CURRENCIES_LIST.find(c => c.id === Currency.CFA).id,
      pricePerKg: undefined!,
      exchangeRate: undefined!,
      stocks: []
    });
  }

  protected addStocks(index: number) {
    this.dialogService.open(
      {
        viewModel: UncommittedStocksDialog,
        model: this.uncommittedStocks
      }
    ).whenClosed(result => {
      if (!result.wasCancelled) {
        const detail = this.model.purchaseDetails[index];
        const stocks = result.output as IStock[];

        detail.stockIds = stocks.map(x => x.id);
        detail.stocks = stocks;
        detail.values = this.purchaseService.getStockAverages(stocks);
      }
    });
  }

  protected deleteStockDetail(detail: IPurchaseDetail, index: number) {
    detail.stockIds.splice(index, 1);
    detail.stocks.splice(index);
  }
  protected selectedSupplierChanged() {
    if (this.model) {
      this.model.supplierId = this.selectedSupplier.id;
    }
  }

  protected get canSave() {
    if (!this.model) {
      return false;
    }

    const header = this.model.supplierId && this.model.purchaseDate && this.model.quantityMt > 0;
    const details = this.model?.purchaseDetails?.length > 0 && this.model.purchaseDetails.every(detail => detail.pricePerKg > 0 && detail?.stockIds?.length > 0);

    return header && details;

  }

  protected async save() {
    if (this.canSave) {
      await this.purchaseService.save(this.model);
    }
  }

  protected listItemMatcher = (a: IListItem, b: IListItem) => a?.id === b?.id;
}
