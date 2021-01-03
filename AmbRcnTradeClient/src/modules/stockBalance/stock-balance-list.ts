import { DialogService } from "aurelia-dialog";
import { autoinject, observable } from "aurelia-framework";
import { connectTo } from "aurelia-store";
import { encodeParams } from "core/helpers";
import _ from "lodash";
import { StockManagementService } from "services/stock-management-service";
import { IState } from "store/state";
import { ContainerStatus } from "./../../constants/app-constants";
import { IAvailableContainer } from "./../../interfaces/stockManagement/IAvailableContainerItem";
import { IStockBalance } from "./../../interfaces/stocks/IStockBalance";
import { StockService } from "./../../services/stock-service";
import { StuffContainerDialog } from "./stuff-container-dialog";

@autoinject
@connectTo()
export class StockBalanceList {
  @observable public state: IState = undefined!;
  protected list: IStockBalance[] = [];
  protected availableContainersList: IAvailableContainer[] = [];
  protected numberEmptyContainers = 0;

  constructor(
    private stockService: StockService,
    private dialogService: DialogService,
    private stockManagementService: StockManagementService
  ) { }

  public async activate(prms: { lotNo: number, locationId: string }) {
    await this.stockManagementService.getAvailableContainers();
    await this.stockService.loadStockBalanceList(prms?.lotNo, prms?.locationId);
  }

  protected stateChanged(state: IState) {
    this.list = _.cloneDeep(state.stock.stockBalanceList);
    this.availableContainersList = _.cloneDeep(state.stockManagement.availableContainers);
    this.numberEmptyContainers = this.availableContainersList.filter(c => c.status === ContainerStatus.Empty).length;
  }

  protected encode(value: string) {
    return encodeParams(value);
  }

  protected async openStuffContainerDialog(stockBalance: IStockBalance) {
    await this.dialogService.open(
      {
        viewModel: StuffContainerDialog,
        model: { stockBalance }
      }
    ).whenClosed(async result => {
      if (!result.wasCancelled) {
        const { container, bags, stockWeightKg } = result.output as { container: IAvailableContainer; bags: number; stockWeightKg: number; };
        const request = this.stockManagementService.getStuffingRequest(container.id, stockBalance, bags, stockWeightKg);
        await this.stockManagementService.stuffContainer(request);
        await this.stockService.loadStockBalanceList(null, null);
      }
    });
  }
}
