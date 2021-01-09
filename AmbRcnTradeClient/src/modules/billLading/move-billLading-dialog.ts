import { DialogController } from "aurelia-dialog";
import { autoinject } from "aurelia-framework";
import _ from "lodash";
import { IState } from "store/state";
import { IVesselListItem } from "./../../interfaces/shipping/IVesselListItem";
import { BillLadingService } from "./../../services/bill-lading-service";

@autoinject
export class MoveBillLadingDialog {
  protected model: { state: IState; billLadingId: string; fromVesselId: string } = { state: undefined!, billLadingId: undefined!, fromVesselId: undefined! };

  protected vesselList: IVesselListItem[] = [];
  protected selectedVessel: IVesselListItem = undefined!;

  constructor(
    private billLadingService: BillLadingService,
    private controller: DialogController
  ) { }

  protected async activate(model: { state: IState; billLadingId: string, fromVesselId: string }) {
    this.model = model;
    this.vesselList = _.cloneDeep(model.state.vessel.list).filter(c => c.id !== model.fromVesselId);
  }

  protected selectRow(item: IVesselListItem) {
    this.vesselList.forEach(c=>c.selected=false);
    item.selected = true;
  }

  protected okClicked() {
    this.selectedVessel = this.vesselList.find(c => c.selected);
    this.controller.ok({ billLadingId: this.model.billLadingId, fromVesselId: this.model.fromVesselId, toVesselId: this.selectedVessel.id });
  }
}
