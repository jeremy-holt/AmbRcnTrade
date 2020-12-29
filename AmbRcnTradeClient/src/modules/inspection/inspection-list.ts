import { App } from "./../../app";
import { encodeParams } from "./../../core/helpers";
import { autoinject, observable } from "aurelia-framework";
import { Router } from "aurelia-router";
import { connectTo } from "aurelia-store";
import _ from "lodash";
import { InspectionService } from "services/inspection-service";
import { IState } from "store/state";
import { Approval, APPROVAL_LIST } from "./../../constants/app-constants";
import { IInspectionListItem } from "./../../interfaces/inspections/IInspectionListItem";

@autoinject
@connectTo()
export class InspectionList {
  @observable public state: IState;
  public list: IInspectionListItem[] = [];
  public approvalList = _.cloneDeep(APPROVAL_LIST);
  @observable selectedApproval = undefined!;

  constructor(
    private inspectionService: InspectionService,
    private router: Router
  ) {

  }

  protected stateChanged(state: IState) {
    this.list = _.cloneDeep(state.inspection.list);

    this.list.forEach(c=>{
      c.css = `text-right text-white ${c.approved === Approval.Approved ? "bg-success" : "bg-danger"}`;
    });
  }

  protected async activate(params: { approval: Approval }) {
    // await this.inspectionService.loadList(params.approval);
  }

  protected bind() {
    this.approvalList.unshift({ id: null, name: "[All]" });
    this.selectedApproval = this.approvalList[0];
  }

  protected async selectedApprovalChanged() {
    await this.inspectionService.loadList(this.selectedApproval.id);
  }

  protected addInspection() {
    this.router.navigateToRoute("inspectionEdit");
  }

  protected encode(value: string) {
    return encodeParams(value);
  }

}
