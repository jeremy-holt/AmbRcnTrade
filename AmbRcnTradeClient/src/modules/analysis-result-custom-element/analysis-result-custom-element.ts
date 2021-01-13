import { autoinject, bindable, observable } from "aurelia-framework";
import { Approval, APPROVAL_LIST } from "constants/app-constants";
import { IAnalysisResult } from "interfaces/inspections/IAnalysis";
import { encodeParams } from "./../../core/helpers";

@autoinject
export class AnalysisResultCustomElement {
  @bindable public model: IAnalysisResult = undefined!
  @bindable protected approvalChecked = false;
  @bindable public caption = "";
  @bindable public inspectionId = "";
  @bindable public index = 1;
  @bindable public disabled = "";

  public approvalList = APPROVAL_LIST;
  @observable selectedApproval = APPROVAL_LIST[0];

  protected approvalLabel = this.selectedApproval.name;

  protected approvalCheckedChanged() {
    this.selectedApproval = this.approvalChecked ? APPROVAL_LIST[0] : APPROVAL_LIST[1];
    this.approvalLabel = this.selectedApproval.name;
  }

  protected get approvalCss() {
    if (this.selectedApproval) {
      let disabledColor = "";

      if (this.disabled) {
        disabledColor = " bg-secondary";
      } else {
        disabledColor = this.selectedApproval.id === Approval.Approved ? " bg-success " : " bg-danger";
      }

      return `text-white rounded px-2 ${disabledColor}`;
    }
  }

  protected get isInValidAnalysis(){
    return !this.model.count || !this.model.kor || !this.model.moisture || !this.model.rejectsPct || !this.model.soundPct || !this.model.spottedPct;
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
