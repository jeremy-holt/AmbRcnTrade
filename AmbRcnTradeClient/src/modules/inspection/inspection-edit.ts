import { APPROVAL_LIST } from "./../../constants/app-constants";
import { IParamsId } from "./../../interfaces/IParamsId";
import { InspectionService } from "./../../services/inspection-service";
import { IInspection } from "./../../interfaces/inspections/IInspection";
import { autoinject, observable } from "aurelia-framework";
import { connectTo } from "aurelia-store";
import { IState } from "store/state";
import _ from "lodash";

@autoinject
@connectTo()
export class InspectionEdit {
  @observable protected state: IState;
  public model: IInspection;
  public approvalList = APPROVAL_LIST;

  constructor(
    private inspectionService: InspectionService
  ) {

  }

  protected stateChanged(state: IState) {
    this.model = _.cloneDeep(state.inspection.current);
  }

  protected async activate(params: IParamsId) {
    if (params?.id) {
      await this.inspectionService.load(params.id);
    } else {
      this.model = {} as IInspection;
    }
  }

  protected get canSave() {
    return true;
  }

  protected async save() {
    if (this.canSave) {
      await this.inspectionService.save(this.model);
    }
  }
}
