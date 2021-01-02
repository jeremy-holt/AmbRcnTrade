import { IAvailableContainerItem } from "./../../interfaces/stockManagement/IAvailableContainerItem";
import { StockManagementService } from "services/stock-management-service";
import { encodeParams } from "core/helpers";
import { StuffContainerDialog } from "./stuff-container-dialog";
import { DialogService } from "aurelia-dialog";
import { StockService } from "./../../services/stock-service";
import { IStockBalanceListItem } from "./../../interfaces/stocks/IStockBalanceListItem";
import { autoinject, observable } from "aurelia-framework";
import { connectTo } from "aurelia-store";
import { IState } from "store/state";
import _ from "lodash";

@autoinject
@connectTo()
export class StockBalanceList {
  @observable public state: IState = undefined!;
  protected list: IStockBalanceListItem[] = [];

  constructor(
    private stockService: StockService,
    private dialogService: DialogService,
    private stockManagementService: StockManagementService
  ) { }

  public async activate(prms: { lotNo: number, locationId: string }) {
    await this.stockService.loadStockBalanceList(prms?.lotNo, prms?.locationId);
  }

  protected stateChanged(state: IState) {
    this.list = _.cloneDeep(state.stock.stockBalanceList);
  }

  protected encode(value: string) {
    return encodeParams(value);
  }

  protected openStuffContainerDialog(stockBalance: IStockBalanceListItem) {
    this.dialogService.open(
      {
        viewModel: StuffContainerDialog,
        model: { stockBalance }
      }
    ).whenClosed(async result => {
      if (!result.wasCancelled) {
        const { container, bags, stockWeightKg } = result.output as { container: IAvailableContainerItem; bags: number; stockWeightKg: number; };
        const request = this.stockManagementService.getStuffingRequest(stockBalance, container, bags, stockWeightKg);
        await this.stockManagementService.stuffContainer(request);
      }
    });
  }
}
