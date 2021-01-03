import { encodeParams } from "./../../core/helpers";
import { ContainerService } from "./../../services/container-service";
import { DialogController } from "aurelia-dialog";
import { autoinject, observable } from "aurelia-framework";
import { connectTo } from "aurelia-store";
import _ from "lodash";
import { StockManagementService } from "services/stock-management-service";
import { IState } from "store/state";
import { IAvailableContainer } from "./../../interfaces/stockManagement/IAvailableContainerItem";
import { IStockBalance } from "./../../interfaces/stocks/IStockBalance";
import { Router } from "aurelia-router";

@autoinject
@connectTo()
export class StuffContainerDialog {
  public model: IStockBalance;
  @observable protected state: IState = undefined!;
  public list: IAvailableContainer[] = [];
  @observable public bags: number;
  public stockWeightKg: number;

  constructor(
    private controller: DialogController,
    private stocksManagementService: StockManagementService,
    private router: Router
  ) { }

  protected async activate(model: { stockBalance: IStockBalance }) {
    this.model = model.stockBalance;
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

  protected selectedContainer() {
    return this.list.find(c => c.selected);
  }

  protected bagsChanged(value: number) {
    this.stockWeightKg = Math.floor(+value * 80);
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

  protected okClicked() {
    this.controller.ok({
      container: this.selectedContainer(),
      bags: this.bags,
      stockWeightKg: this.stockWeightKg
    });
  }

  protected encode(value: string) {
    return encodeParams(value);
  }

  protected navigateToContainer(containerId: string) {
    this.router.navigateToRoute("containerEdit", { id: encodeParams(containerId) });
    this.controller.cancel();
  }
}
