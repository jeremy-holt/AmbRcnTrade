import { encodeParams } from "./../../core/helpers";
import { VesselService } from "./../../services/vessel-service";
import { IVesselListItem } from "./../../interfaces/shipping/IVesselListItem";
import { autoinject, observable } from "aurelia-framework";
import { connectTo } from "aurelia-store";
import { IState } from "store/state";
import { Router } from "aurelia-router";
import _ from "lodash";
import { CustomerService } from "services/customer-service";

@autoinject
@connectTo()
export class VesselList {
  @observable protected state: IState = undefined!;
  protected list: IVesselListItem[] = [];

  constructor(
    private vesselService: VesselService,
    private customerService: CustomerService,
    private router: Router
  ) { }

  protected async activate() {
    await this.vesselService.loadList();
    await this.customerService.loadCustomersForAppUserList();
  }

  protected stateChanged(state: IState): void {
    this.list = _.cloneDeep(state.vessel.list);
  }

  protected addVessel() {
    this.router.navigateToRoute("vesselEdit", { id: null });
  }

  protected encode(value: string){
    return encodeParams(value);
  }

  protected navigateToContainersList(){
    this.router.navigateToRoute("containerList");
  }
}
