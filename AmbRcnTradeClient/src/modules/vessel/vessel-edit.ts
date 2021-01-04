import { DialogService } from "aurelia-dialog";
import { DATEFORMAT } from "./../../constants/app-constants";
import { IParamsId } from "./../../interfaces/IParamsId";
import { Router } from "aurelia-router";
import { IVessel } from "interfaces/shipping/IVessel";
import { autoinject, observable } from "aurelia-framework";
import { connectTo } from "aurelia-store";
import { IState } from "store/state";
import { VesselService } from "services/vessel-service";
import _ from "lodash";
import { IEtaHistory } from "interfaces/shipping/IEtaHistory";
import moment from "moment";

@autoinject
@connectTo()
export class VesselEdit {
  @observable public state: IState = undefined!
  protected model: IVessel = undefined!;

  constructor(
    private vesselService: VesselService,
    private router: Router,
    private dialogService: DialogService
  ) { }

  protected async activate(prms: IParamsId) {
    if (prms?.id) {
      await this.vesselService.load(prms?.id);
    } else {
      await this.vesselService.createVessel();
    }
  }

  protected stateChanged(state: IState): void {
    this.model = _.cloneDeep(state.vessel.current);
    if (this.model?.etaHistory.length === 0) {
      this.addEtaHistory();
    }
  }

  protected get canSave() {
    return this.model?.etaHistory.length > 0;
  }

  protected async save() {
    if (this.canSave) {
      await this.vesselService.save(this.model);
    }
  }

  protected addEtaHistory(){
    this.model.etaHistory.push({ dateUpdated: moment().format(DATEFORMAT) } as IEtaHistory);
  }

  protected addContainersDialog() {
    alert("Will add containers");
  }
}
