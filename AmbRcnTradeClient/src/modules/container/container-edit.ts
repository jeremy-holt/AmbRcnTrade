import { DeleteDialog } from "./../../dialogs/delete-dialog";
import { UnstuffContainerDialog } from "./unstuff-container-dialog";
import { DialogService } from "aurelia-dialog";
import { Router } from "aurelia-router";
import { CONTAINER_STATUS_LIST, IContainerStatus, TEU_LIST } from "./../../constants/app-constants";
import { IParamsId } from "interfaces/IParamsId";
import { ContainerService } from "./../../services/container-service";
import { autoinject, observable } from "aurelia-framework";
import { connectTo } from "aurelia-store";
import { IContainer } from "interfaces/shipping/IContainer";
import { IState } from "store/state";
import _ from "lodash";

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

  protected bind() {
    this.selectedContainerStatus = this.containerStatusList.find(c => c.id === this.model.status);
  }

  protected get canSave() {
    return this.model?.containerNumber !== null;
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
    return this.containerService.canUnstuffContainer(this.model);
  }

  protected get canDeleteContainer() {
    return this.model?.incomingStocks.length === 0;
  }

  protected deleteContainer() {
    this.dialogService.open({ viewModel: DeleteDialog }).whenClosed(async result => {
      if (!result.wasCancelled) {
        await this.containerService.deleteContainer(this.model.id);
        this.router.navigateToRoute("containerList");
      }
    });
  }
}
