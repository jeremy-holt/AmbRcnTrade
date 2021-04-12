import { getRavenRootId } from "./../../core/helpers";
import { CustomerService } from "./../../services/customer-service";
import { ICustomerListItem } from "./../../interfaces/ICustomerListItem";
import { autoinject, observable } from "aurelia-framework";
import { Router } from "aurelia-router";
import { connectTo } from "aurelia-store";
import { encodeParams } from "core/helpers";
import _ from "lodash";
import { IState } from "store/state";
import { CONTAINER_STATUS_LIST, CustomerGroup, IContainerStatus } from "./../../constants/app-constants";
import { IContainer } from "./../../interfaces/shipping/IContainer";
import { ContainerService } from "./../../services/container-service";
import { isInRole } from "./../../services/role-service";
import { ContainerStatusFormatterValueConverter } from "./container-status-formatter-valueconverter";

@autoinject
@connectTo()
export class ContainerList {
  @observable public state: IState = undefined!;
  protected list: IContainer[] = [];
  protected containerStatusList = _.cloneDeep(CONTAINER_STATUS_LIST);
  @observable protected selectedContainerStatus: IContainerStatus = undefined!;
  protected containerSummary: { name: string, count: number }[] = [];
  protected warehouses: ICustomerListItem[] = [];

  constructor(
    private containerService: ContainerService,
    private router: Router,
    private containerStatusFormatter: ContainerStatusFormatterValueConverter,
    private customerService: CustomerService
  ) { }

  protected async activate() {
    await this.customerService.loadCustomersForAppUserList();
    this.containerStatusList.unshift({ id: null, name: "[All]" });
  }

  protected stateChanged(state: IState) {
    this.warehouses = _.cloneDeep(state.userFilteredCustomers.filter(c => c.filter === CustomerGroup.Warehouse));
    this.warehouses.unshift({ id: null, name: "[All]" } as ICustomerListItem);

    this.list = _.cloneDeep(state.container.list);

    this.setContainersSummary();
  }

  protected setContainersSummary() {
    const summary: { name: string, count: number }[] = [];

    const groupedList = _.groupBy(this.list, "status");
    let k: keyof typeof groupedList;
    for (k in groupedList) {
      const v = groupedList[k];
      summary.push({ name: this.containerStatusFormatter.toView(k), count: v.length });
    }

    this.containerSummary = summary;
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

  protected navigateToStockBalanceList() {
    this.router.navigateToRoute("stockBalanceList");
  }

  protected navigateToVesselList() {
    this.router.navigateToRoute("vesselList");
  }

  protected getRavenRootId(id: string){
    return getRavenRootId(id);
  }
}
