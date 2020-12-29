import { autoinject, observable } from "aurelia-framework";
import { connectTo } from "aurelia-store";
import { Approval } from "constants/app-constants";
import { IAnalysis } from "interfaces/inspections/IAnalysis";
import _ from "lodash";
import { IState } from "store/state";
import { APPROVAL_LIST } from "./../../constants/app-constants";
import { IListItem } from "./../../interfaces/IEntity";
import { IInspection } from "./../../interfaces/inspections/IInspection";
import { IParamsId } from "./../../interfaces/IParamsId";
import { CustomerService } from "./../../services/customer-service";
import { InspectionService } from "./../../services/inspection-service";

@autoinject
@connectTo()
export class InspectionEdit {
  @observable protected state: IState;
  public model: IInspection = undefined!;
  public approvalList = APPROVAL_LIST;
  @observable selectedApproval = APPROVAL_LIST[0];
  protected suppliers: IListItem[] = [];
  protected showAnalyses = false;
  @observable protected selectedSupplier: IListItem = undefined!;
  @observable protected approvalChecked = false;
  protected approvalLabel = this.selectedApproval.name;

  constructor(
    private inspectionService: InspectionService,
    private customerService: CustomerService
  ) { }

  protected stateChanged(state: IState) {
    this.model = _.cloneDeep(state.inspection.current);
    this.suppliers = _.cloneDeep(state.userFilteredCustomers);
    this.suppliers.unshift({ id: null, name: "[Select]" } as IListItem);
  }

  protected async activate(params: IParamsId) {
    await this.customerService.loadCustomersForAppUserList();

    if (params?.id) {
      await this.inspectionService.load(params.id);
    } else {
      await this.inspectionService.createInspection();
    }
  }

  protected approvalCheckedChanged() {
    this.selectedApproval = this.approvalChecked ? APPROVAL_LIST[0] : APPROVAL_LIST[1];
    this.approvalLabel = this.selectedApproval.name;
  }

  protected get approvalCss() {
    if (this.selectedApproval) {
      return this.selectedApproval.id === Approval.Approved ? "bg-success rounded" : "bg-danger text-white rounded";
    }
  }

  protected async deactivate() {
    await this.save();
  }

  protected selectedApprovalChanged() {
    if (this.model) {
      this.model.approved = this.selectedApproval.id;
    }
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
      c => c.count === undefined! || c.bags === undefined ||
        c.moisture === undefined || c.soundGm === undefined ||
        c.rejectsGm === undefined || c.spottedGm === undefined ||
        c.kor === undefined);

  }

  protected get canAddAnalysis() {
    return this.model?.inspectionDate && this.model?.inspector && this.model?.bags > 0 && this.canSaveAnalysis;
  }

  protected addAnalysis() {
    this.model.analyses.push({
      approved: Approval.Rejected
    } as IAnalysis);

    this.showAnalyses = true;
  }

  protected removeRow(index: number) {
    this.model.analyses.splice(index, 1);
    this.calc();

    if (this.model.analyses.length === 0) {
      this.approvalChecked = false;
    }
  }

  protected calc() {
    if (this.model.analysisResult) {
      this.model.analysisResult = this.inspectionService.getAnalysisResult(this.model.analyses, this.model.analysisResult.approved);
    }
  }

  protected showHideAnalysis() {
    this.showAnalyses = !this.showAnalyses;
  }

  protected get showAnalysesCaption() {
    return this.showAnalyses ? "Hide analyses" : "Show analysis";
  }

  protected listItemMatcher = (a: IListItem, b: IListItem) => a?.id === b?.id;
}
