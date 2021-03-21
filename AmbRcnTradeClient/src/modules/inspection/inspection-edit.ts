import { InfoDialog } from "./../../dialogs/info-dialog";
import { BillLadingUploadDialog } from "./../billLadingUploadDialog/billLading-upload-dialog";
import { DialogService } from "aurelia-dialog";
import { Subscription } from "aurelia-event-aggregator";
import { autoinject, observable } from "aurelia-framework";
import { Router } from "aurelia-router";
import { connectTo } from "aurelia-store";
import { Approval } from "constants/app-constants";
import { ICustomerListItem } from "interfaces/ICustomerListItem";
import { IAnalysis } from "interfaces/inspections/IAnalysis";
import _ from "lodash";
import { IState } from "store/state";
import { encodeParams, getRavenRootId } from "./../../core/helpers";
import { IListItem } from "./../../interfaces/IEntity";
import { IInspection } from "./../../interfaces/inspections/IInspection";
import { IParamsId } from "./../../interfaces/IParamsId";
import { IMoveInspectionToStockRequest } from "./../../interfaces/stockManagement/IMoveInspectionToStockRequest";
import { CustomerService } from "./../../services/customer-service";
import { InspectionService } from "./../../services/inspection-service";
import { isInRole } from "./../../services/role-service";
import { StockManagementService } from "./../../services/stock-management-service";
import { AddToStockDialog } from "./add-to-stock-dialog";
import { DocumentsDownloadDialog } from "modules/documents-download-dialog/documents-download-dialog";

@autoinject
@connectTo()
export class InspectionEdit {
  @observable protected state: IState;
  public model: IInspection = undefined!;

  @observable protected approvalChecked = false;

  protected suppliers: IListItem[] = [];
  private isDeleting = false;

  private subscriptions: Subscription[] = [];

  constructor(
    private inspectionService: InspectionService,
    private customerService: CustomerService,
    private stockManagementService: StockManagementService,
    private router: Router,
    private dialogService: DialogService
  ) {
  }

  protected stateChanged(state: IState) {
    this.model = _.cloneDeep(state.inspection.current);
    this.suppliers = _.cloneDeep(state.userFilteredCustomers);
    this.suppliers.unshift({ id: null, name: "[Select]", filter: null } as ICustomerListItem);

    if (this.model) {
      this.approvalChecked = this.model?.analysisResult.approved === Approval.Approved;
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

  protected get canSave() {
    return this.canAddAnalysis && this.model?.analyses?.length > 0 && this.canSaveAnalysis && !this.isDeleting && this.model?.supplierId?.length > 0;
  }

  protected async save() {
    if (this.canSave) {
      if (this.inspectionService.existsFiche(this.model, this.state.inspection.list)) {
        this.dialogService.open({
          viewModel: InfoDialog,
          model: { header: "Duplicate Inspection", body: `There already exists an Inspection with fiche ${this.model.fiche}` }
        });
      } else {
        this.model.userName = this.state.user.firstName;
        await this.inspectionService.save(this.model);
      }
    }
  }

  protected get canSaveAnalysis() {
    return this.model && !this.model.analyses.some(
      c => c.count === undefined! || c.count === 0 ||
        c.moisture === undefined || c.soundGm === undefined || c.moisture === 0 || c.soundGm === 0 ||
        c.rejectsGm === undefined || c.spottedGm === undefined ||
        c.kor === undefined);

  }

  protected get canAddAnalysis() {
    return this.model?.inspectionDate && this.model?.inspector && this.canSaveAnalysis;
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
    return this.inspectionService.canAddInspectionToStock(this.model) && this.wasInspectionApproved && isInRole(["admin", "user", "warehouseManager"], this.state);
  }

  protected get canChangeApproval() {
    return this.model.stockReferences.length === 0;
  }

  protected get wasInspectionApproved() {
    return this.inspectionService.inspectionApproved(this.model);
  }

  protected get showCanAddInspectionToStock() {
    return isInRole(["admin", "user", "warehouseManager"], this.state);
  }

  protected async deleteInspection() {
    this.isDeleting = true;
    await this.inspectionService.deleteInspection(this.model.id);
    this.router.navigateToRoute("inspectionList");
  }

  protected get canDeleteInspection() {
    return isInRole(["admin", "user", "warehouseManager"], this.state) && this.model.id?.length > 0
      && this.model?.stockReferences?.length === 0;
  }

  protected async openAddToStockDialog() {
    await this.save();

    this.dialogService.open(
      {
        viewModel: AddToStockDialog,
        model: { inspection: this.model, supplierName: this.suppliers.find(c => c.id === this.model.supplierId)?.name }
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

  protected get remainingKgToAllocate() {
    return this.model.weightKg - this.inspectionService.weightKgAlreadyAllocated(this.model, this.avgBagWeightKg);
  }

  protected get avgBagWeightKg() {
    return this.model.bags > 0 ? this.model.weightKg / this.model.bags : 0;
  }

  protected encode(value: string) {
    return encodeParams(value);
  }

  protected listItemMatcher = (a: IListItem, b: IListItem) => a?.id === b?.id;

  protected navigateToStockList() {
    this.router.navigateToRoute("stockList");
  }

  protected detached() {
    this.subscriptions.forEach(c => c.dispose());
  }

  protected getInspectionNumber(id: string) {
    return getRavenRootId(id);
  }

  protected async uploadDocuments() {
    await this.dialogService.open({
      viewModel: BillLadingUploadDialog,
      model: { billLadingId: this.model.id }
    });
  }

  protected get userName(){
    return this.state?.user.name;
  }

  protected async downloadDocuments() {
    await this.dialogService.open({
      viewModel: DocumentsDownloadDialog,
      model: { billLadingId: this.model.id }
    }).whenClosed();
  }
}
