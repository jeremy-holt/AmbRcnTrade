import { Router } from "aurelia-router";
import { Approval, APPROVAL_LIST } from "./../../constants/app-constants";
import { IInspectionListItem } from "./../../interfaces/inspections/IInspectionListItem";
import { IInspection } from "./../../interfaces/inspections/IInspection";
import { autoinject, observable } from "aurelia-framework";
import { connectTo } from "aurelia-store";
import { InspectionService } from "services/inspection-service";
import { IState } from "store/state";
import _ from "lodash";

@autoinject
@connectTo()
export class InspectionList {
  @observable public state: IState;
  public list: IInspectionListItem[] = [];
  public approvalList=APPROVAL_LIST;
  @observable selectedApproval = APPROVAL_LIST[0];

  constructor(
    private inspectionService: InspectionService,
    private router: Router
  ) { }

  protected stateChanged(state: IState) {
    this.list = _.cloneDeep(state.inspection.list);
  }

  protected async activate(params: {approval: Approval}) { 
    await this.inspectionService.loadList(params.approval);
  }

  protected addInspection(){
    this.router.navigateToRoute("inspectionEdit");
  }
}
