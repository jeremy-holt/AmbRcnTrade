import { isInRole } from "./../../services/role-service";
import { Router } from "aurelia-router";
import { encodeParams } from "core/helpers";
import { autoinject, observable } from "aurelia-framework";
import { connectTo } from "aurelia-store";
import _ from "lodash";
import { IState } from "store/state";
import { CONTAINER_STATUS_LIST, IContainerStatus } from "./../../constants/app-constants";
import { IContainer } from "./../../interfaces/shipping/IContainer";
import { ContainerService } from "./../../services/container-service";

@autoinject
@connectTo()
export class ContainerList {
  @observable public state: IState = undefined!;
  protected list: IContainer[] = [];
  protected containerStatusList = _.cloneDeep(CONTAINER_STATUS_LIST);
  @observable protected selectedContainerStatus: IContainerStatus = undefined!;

  constructor(
    private containerService: ContainerService,
    private router: Router
  ) { }

  protected async activate() {
    this.containerStatusList.unshift({ id: null, name: "[All]" });
  }

  protected stateChanged(state: IState) {
    this.list = _.cloneDeep(state.container.list);
  }

  protected async selectedContainerStatusChanged(status: IContainerStatus) {
    await this.containerService.loadList(status.id);
  }

  protected encode(value: string) {
    return encodeParams(value);
  }

  protected addContainer() {
    this.router.navigateToRoute("containerEdit");
  }

  protected get canAddContainer() {
    return isInRole(["admin", "user", "warehouseManager"], this.state);
  }

  protected navigateToStockBalanceList(){
    this.router.navigateToRoute("stockBalanceList");
  }

  protected navigateToVesselList(){
    this.router.navigateToRoute("vesselList");
  }
}
