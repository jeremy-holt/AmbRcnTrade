import { BillLadingUploadDialog } from "./../billLadingUploadDialog/billLading-upload-dialog";
import { DeleteDialog } from "./../../dialogs/delete-dialog";
import { DialogService } from "aurelia-dialog";
import { encodeParams, getRavenRootId } from "core/helpers";
import { autoinject, observable } from "aurelia-framework";
import { Router } from "aurelia-router";
import { connectTo } from "aurelia-store";
import { IListItem } from "core/interfaces/IEntity";
import { IParamsId } from "core/interfaces/IParamsId";
import { IStock } from "interfaces/stocks/IStock";
import _ from "lodash";
import { IState } from "store/state";
import { CustomerService } from "./../../services/customer-service";
import { StockService } from "./../../services/stock-service";
import { DocumentsDownloadDialog } from "modules/documents-download-dialog/documents-download-dialog";

@autoinject
@connectTo()
export class StockEdit {
  @observable protected state: IState = undefined!;
  protected model: IStock = undefined!;
  protected locations: IListItem[] = [];
  protected suppliers: IListItem[] = [];

  protected locationName = "";
  protected supplierName = "";

  constructor(
    private stockService: StockService,
    private customerService: CustomerService,
    private dialogService: DialogService,
    private router: Router
  ) { }

  protected stateChanged(state: IState) {
    this.locations = _.cloneDeep(state.userFilteredCustomers);
    this.suppliers = _.cloneDeep(state.userFilteredCustomers);

    this.locations.unshift({ id: null, name: "[Select]" });
    this.suppliers.unshift({ id: null, name: "[Select]" });

    this.model = _.cloneDeep(state.stock.current);
    if (this.model && this.locations?.length > 0) {
      this.locationName = this.locations.find(c => c.id === this.model.locationId)?.name;
      this.supplierName = this.suppliers.find(c => c.id === this.model.supplierId)?.name;
    }
  }

  protected async activate(params: IParamsId) {
    await this.customerService.loadCustomersForAppUserList();

    if (params?.id) {
      await this.stockService.load(params.id);
    }
  }

  protected encode(value: string) {
    return encodeParams(value);
  }

  protected get canDelete() {
    return this.model?.isStockIn && this.model?.stuffingRecords.length === 0;
  }

  protected deleteStock() {
    this.dialogService.open({
      viewModel: DeleteDialog,
      model: {
        header: "Undo stock",
        body: "Are you sure you want to undo this stock? After deleting it the inspection will be shown as unallocated"
      }
    }).whenClosed(async result => {
      if (!result.wasCancelled) {
        await this.stockService.deleteStock(this.model.id);
        this.router.navigateToRoute("stockList");
      }
    });
  }

  protected async uploadDocuments() {
    await this.dialogService.open({
      viewModel: BillLadingUploadDialog,
      model: { billLadingId: this.model.id }
    });
  }

  protected async downloadDocuments() {
    await this.dialogService.open({
      viewModel: DocumentsDownloadDialog,
      model: { billLadingId: this.model.id }
    }).whenClosed();
  }

  protected getInspectionNumber(id: string){
    return getRavenRootId(id);
  }
}
