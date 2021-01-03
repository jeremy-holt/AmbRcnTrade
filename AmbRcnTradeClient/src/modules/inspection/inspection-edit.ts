import { isInRole } from "./../../services/role-service";
import { DialogService } from "aurelia-dialog";
import { autoinject, observable } from "aurelia-framework";
import { Router } from "aurelia-router";
import { connectTo } from "aurelia-store";
import { Approval } from "constants/app-constants";
import { IAnalysis } from "interfaces/inspections/IAnalysis";
import _ from "lodash";
import { IState } from "store/state";
import { encodeParams } from "./../../core/helpers";
import { IListItem } from "./../../interfaces/IEntity";
import { IInspection } from "./../../interfaces/inspections/IInspection";
import { IParamsId } from "./../../interfaces/IParamsId";
import { IMoveInspectionToStockRequest } from "./../../interfaces/stockManagement/IMoveInspectionToStockRequest";
import { CustomerService } from "./../../services/customer-service";
import { InspectionService } from "./../../services/inspection-service";
import { StockManagementService } from "./../../services/stock-management-service";
import { AddToStockDialog } from "./add-to-stock-dialog";

@autoinject
@connectTo()
export class InspectionEdit {
  @observable protected state: IState;
  public model: IInspection = undefined!;

  @observable protected approvalChecked = false;

  protected suppliers: IListItem[] = [];
  @observable protected selectedSupplier: IListItem = undefined!;

  constructor(
    private inspectionService: InspectionService,
    private customerService: CustomerService,
    private stockManagementService: StockManagementService,
    private router: Router,
    private dialogService: DialogService
  ) { }

  protected stateChanged(state: IState) {
    this.model = _.cloneDeep(state.inspection.current);
    this.suppliers = _.cloneDeep(state.userFilteredCustomers);
    this.suppliers.unshift({ id: null, name: "[Select]" } as IListItem);

    if (this.model) {
      this.approvalChecked = this.model?.analysisResult.approved === Approval.Approved;
      this.selectedSupplier = this.suppliers.find(c => c.id === this.model.supplierId);
    }
  }

  protected async activate(params: IParamsId) {
    await this.customerService.loadCustomersForAppUserList();

    if (params?.id) {
      await this.inspectionService.load(params.id);
    } else {
      await this.inspectionService.createInspection();
    }

  }

  protected async deactivate() {
    await this.save();
  }

  protected selectedSupplierChanged() {
    if (this.model) {
      this.model.supplierId = this.selectedSupplier?.id;
    }
  }

  protected get canSave() {
    return this.canAddAnalysis && this.model?.analyses?.length > 0 && this.canSaveAnalysis;
  }

  protected async save() {
    if (this.canSave) {
      await this.inspectionService.save(this.model);
    }
  }

  protected get canSaveAnalysis() {
    return this.model && !this.model.analyses.some(
      c => c.count === undefined! ||
        c.moisture === undefined || c.soundGm === undefined ||
        c.rejectsGm === undefined || c.spottedGm === undefined ||
        c.kor === undefined);

  }

  protected get canAddAnalysis() {
    return this.model?.inspectionDate && this.model?.inspector && this.model?.bags > 0 && this.canSaveAnalysis;
  }

  protected addAnalysis() {
    this.model.analyses.push({} as IAnalysis);
  }

  protected removeRow(index: number) {
    this.model.analyses.splice(index, 1);
    this.calc();

    if (this.model.analyses.length === 0) {
      this.approvalChecked = false;
    }
  }

  protected addInspection() {
    this.router.navigateToRoute("inspectionEdit", { id: null });
  }

  protected calc() {
    if (this.model.analysisResult) {
      this.model.analysisResult = this.inspectionService.getAnalysisResult(this.model.analyses, this.model.analysisResult.approved);
    }
  }

  protected get canAddInspectionToStock() {
    if (!this.model) {
      return false;
    }
    return this.inspectionService.canAddInspectionToStock(this.model);
  }

  protected get showCanAddInspectionToStock() {
    return isInRole(["warehouseManager"], this.state);
  }

  protected async openAddToStockDialog() {
    this.dialogService.open(
      {
        viewModel: AddToStockDialog,
        model: { inspection: this.model, supplierName: this.selectedSupplier.name }
      }
    ).whenClosed(async result => {
      if (!result.wasCancelled) {
        const request = result.output as IMoveInspectionToStockRequest;

        await this.stockManagementService.moveInspectionToStock(request);

        if (this.state.inspection.movedToStockId) {
          this.router.navigateToRoute("stockEdit", { id: this.encode(this.state.inspection.movedToStockId) });
        }
      }
    });
  }

  protected get remainingBagsToAllocate() {
    return this.model.bags - this.inspectionService.bagsAlreadyAllocated(this.model);
  }

  protected encode(value: string) {
    return encodeParams(value);
  }

  protected listItemMatcher = (a: IListItem, b: IListItem) => a?.id === b?.id;

  protected navigateToStockList(){
    this.router.navigateToRoute("stockList");
  }
}
