import { encodeParams } from "./../../core/helpers";
import { DialogService } from "aurelia-dialog";
import { autoinject, observable } from "aurelia-framework";
import { Router } from "aurelia-router";
import { connectTo } from "aurelia-store";
import { IParamsId } from "interfaces/IParamsId";
import { IContainer } from "interfaces/shipping/IContainer";
import _ from "lodash";
import { IState } from "store/state";
import { CONTAINER_STATUS_LIST, IContainerStatus, TEU_LIST } from "./../../constants/app-constants";
import { DeleteDialog } from "./../../dialogs/delete-dialog";
import { IVessel } from "./../../interfaces/shipping/IVessel";
import { ContainerService } from "./../../services/container-service";
import { isInRole } from "./../../services/role-service";
import { VesselService } from "./../../services/vessel-service";
import { UnstuffContainerDialog } from "./unstuff-container-dialog";

@autoinject
@connectTo()
export class ContainerEdit {
  @observable protected state: IState = undefined!;
  protected model: IContainer = undefined!;
  protected containerStatusList = CONTAINER_STATUS_LIST;
  protected teuList = TEU_LIST;
  @observable protected selectedContainerStatus: IContainerStatus = undefined;
  
  constructor(
    private containerService: ContainerService,    
    private dialogService: DialogService,
    private router: Router
  ) { }

  protected async activate(prms: IParamsId) {   
    if (prms?.id) {
      await this.containerService.load(prms.id);
    } else {
      await this.containerService.createContainer();
    }    
  }

  protected stateChanged(state: IState) {
    this.model = _.cloneDeep(state.container.current);
  
    this.teuList = _.cloneDeep(TEU_LIST);
    this.teuList.unshift({ id: null, name: "[Select]" });
  }

  protected async bind() {
    this.selectedContainerStatus = this.containerStatusList.find(c => c.id === this.model.status);
    // await this.vesselService.load(this.model.vesselId);
  }

  protected get canSave() {
    return this.model?.containerNumber !== null && this.canEditContainer;
  }

  protected async save() {
    if (this.canSave) {
      this.model.status = this.selectedContainerStatus.id;
      await this.containerService.save(this.model);
      this.router.navigateToRoute("containerList");
    }
  }

  protected unstuffContainer() {
    this.dialogService.open(
      {
        viewModel: UnstuffContainerDialog
      }
    ).whenClosed(async result => {
      if (!result.wasCancelled) {
        await this.containerService.unstuffContainer({ containerId: this.model.id });
        await this.containerService.load(this.model.id);
      }
    });
  }

  protected get canUnstuffContainer() {
    return this.containerService.canUnstuffContainer(this.model) && isInRole(["admin", "user", "warehouseManager"], this.state);
  }

  protected get canDeleteContainer() {
    return this.model?.incomingStocks.length === 0 && isInRole(["admin", "user", "warehouseManager"], this.state);
  }

  protected deleteContainer() {
    this.dialogService.open({ viewModel: DeleteDialog }).whenClosed(async result => {
      if (!result.wasCancelled) {
        await this.containerService.deleteContainer(this.model.id);
        this.router.navigateToRoute("containerList");
      }
    });
  }

  protected get canEditContainer() {
    return isInRole(["admin", "user", "warehouseManager"], this.state);
  }

  protected get canShowDeleteContainer() {
    return this.model?.bags > 0;
  }

  protected async addContainer() {
    await this.containerService.createContainer();
    this.selectedContainerStatus = this.containerStatusList.find(c => c.id === this.model.status);
  }

  protected encode(value: string){
    return encodeParams(value);
  }
}
