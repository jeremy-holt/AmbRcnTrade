import { DialogController } from "aurelia-dialog";
import { autoinject, observable } from "aurelia-framework";
import { Router } from "aurelia-router";
import { connectTo } from "aurelia-store";
import { ContainerStatus, DATEFORMAT } from "constants/app-constants";
import { IStuffingRequest } from "interfaces/stockManagement/IStuffingRequest";
import _ from "lodash";
import moment from "moment";
import { StockManagementService } from "services/stock-management-service";
import { IState } from "store/state";
import { encodeParams } from "./../../core/helpers";
import { IAvailableContainer } from "./../../interfaces/stockManagement/IAvailableContainerItem";
import { IStockBalance } from "./../../interfaces/stocks/IStockBalance";
import { ContainerService } from "./../../services/container-service";

@autoinject
@connectTo()
export class StuffContainerDialog {
  public model: IStockBalance;
  @observable protected state: IState = undefined!;
  public list: IAvailableContainer[] = [];
  @observable public bags: number;
  public stockWeightKg: number;
  @observable public stuffingStatus = true;
  public stuffingStatusLabel = "Completed stuffing";
  public stuffingDate = moment().format(DATEFORMAT);


  private avgBagWeightKg = 0;
  private containerStatus = ContainerStatus.StuffingComplete;

  constructor(
    private controller: DialogController,
    private stocksManagementService: StockManagementService,
    private router: Router
  ) { }

  protected async activate(model: { stockBalance: IStockBalance }) {
    this.model = model.stockBalance;
    console.log(this.model);
    this.avgBagWeightKg = model.stockBalance.avgBagWeightKg;

    await this.stocksManagementService.getAvailableContainers();
  }

  protected stateChanged(state: IState) {
    this.list = _.cloneDeep(state.stockManagement.availableContainers);
  }

  protected get canSave() {
    return this.list?.filter(c => c.selected).length > 0 && this.bags > 0;
  }

  protected selectRow(index: number) {
    this.list.forEach(c => c.selected = false);
    this.list[index].selected = !this.list[index].selected;
  }

  protected selectedContainer(): IAvailableContainer {
    return this.list.find(c => c.selected);
  }

  protected bagsChanged(value: number) {
    this.stockWeightKg = _.round(+value * this.avgBagWeightKg, 2);
  }

  protected get isOverweightContainer() {
    return ContainerService.isOverweightContainer({ bags: this.loadingQuantity.bags, weightKg: this.loadingQuantity.stockWeightKg });
  }

  protected get loadingQuantity() {
    const container = this.selectedContainer();
    if (!container) {
      return { bags: 0, stockWeightKg: 0 };
    }
    return {
      bags: this.bags + container.bags,
      stockWeightKg: (this.stockWeightKg + container.stockWeightKg)
    };
  }

  protected stuffingStatusChanged(status: boolean) {
    this.stuffingStatusLabel = status ? "Completed stuffing" : "Partially stuffed";
    this.containerStatus = status ? ContainerStatus.StuffingComplete : ContainerStatus.Stuffing;
  }

  protected okClicked() {
    if (this.containerStatus === ContainerStatus.Cancelled) {
      throw new Error("Cannot stuff into a cancelled container");
    }

    const request: IStuffingRequest = {
      containerId: this.selectedContainer()?.id,
      stuffingDate: this.stuffingDate,
      stockBalance: this.model,
      bags: this.bags,
      weightKg: this.stockWeightKg,
      status: this.containerStatus,
    };

    this.controller.ok(request);
  }

  protected encode(value: string) {
    return encodeParams(value);
  }

  protected navigateToContainer(containerId: string) {
    this.router.navigateToRoute("containerEdit", { id: encodeParams(containerId) });
    this.controller.cancel();
  }
}
