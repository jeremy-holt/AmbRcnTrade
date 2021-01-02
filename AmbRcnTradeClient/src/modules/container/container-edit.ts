import { Router } from "aurelia-router";
import { CONTAINER_STATUS_LIST, IContainerStatus } from "./../../constants/app-constants";
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
  @observable protected selectedContainerStatus: IContainerStatus = undefined;

  constructor(
    private containerService: ContainerService,
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
  }

  protected bind() {
    this.selectedContainerStatus = this.containerStatusList.find(c => c.id === this.model.id);
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

}
