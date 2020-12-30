import { encodeParams } from "./../../core/helpers";
import { IAnalysis } from "interfaces/inspections/IAnalysis";
import { autoinject, bindable, observable } from "aurelia-framework";
import { Approval, APPROVAL_LIST } from "constants/app-constants";

@autoinject
export class AnalysisResultCustomElement {
  @bindable public model: IAnalysis = undefined!
  @bindable protected approvalChecked = false;
  @bindable public caption = "";
  @bindable public inspectionId = "";
  @bindable public index = 1;

  public approvalList = APPROVAL_LIST;
  @observable selectedApproval = APPROVAL_LIST[0];

  protected approvalLabel = this.selectedApproval.name;

  protected approvalCheckedChanged() {
    this.selectedApproval = this.approvalChecked ? APPROVAL_LIST[0] : APPROVAL_LIST[1];
    this.approvalLabel = this.selectedApproval.name;
  }

  protected get approvalCss() {
    if (this.selectedApproval) {
      return this.selectedApproval.id === Approval.Approved ? "text-white rounded px-2 bg-success" : "text-white rounded px-2 bg-danger";
    }
  }

  protected selectedApprovalChanged() {
    if (this.model) {
      this.model.approved = this.selectedApproval.id;
    }
  }

  protected encode(value: string) {
    return encodeParams(value);
  }
}
