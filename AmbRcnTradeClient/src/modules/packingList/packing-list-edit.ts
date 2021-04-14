import { encodeParams } from "core/helpers";
import { PortService } from "./../../services/port-service";
import { CustomerService } from "./../../services/customer-service";
import { IPort } from "./../../interfaces/IPort";
import { ICustomerListItem } from "./../../interfaces/ICustomerListItem";
import { IContainer } from "interfaces/shipping/IContainer";
import { getRavenRootId } from "./../../core/helpers";
import { Router } from "aurelia-router";
import { DeleteDialog } from "./../../dialogs/delete-dialog";
import { DialogService } from "aurelia-dialog";
import { autoinject } from "aurelia-dependency-injection";
import { connectTo } from "aurelia-store";
import _ from "lodash";
import { IState } from "store/state";
import { IParamsId } from "./../../interfaces/IParamsId";
import { IPackingList } from "./../../interfaces/shipping/IPackingList";
import { PackingListService } from "./../../services/packing-list-service";
import { CustomerGroup } from "constants/app-constants";

@autoinject
@connectTo()
export class PackingListEdit {
  public model: IPackingList;
  private unallocatedContainers: IContainer[] = [];
  protected shippers: ICustomerListItem[] = [];
  protected freightForwarders: ICustomerListItem[] = [];
  protected warehouses: ICustomerListItem[] = [];
  protected customers: ICustomerListItem[] = [];
  protected ports: IPort[] = [];

  constructor(
    private packingListService: PackingListService,
    private dialogService: DialogService,
    private router: Router,
    private customerService: CustomerService,
    private portService: PortService
  ) { }

  protected stateChanged(state: IState) {
    this.model = _.cloneDeep(state.packingList.current);
    this.unallocatedContainers = _.cloneDeep(state.packingList.unallocatedContainers);

    this.shippers = _.cloneDeep(state.userFilteredCustomers.filter(c => c.filter === CustomerGroup.BillLading));
    this.shippers.unshift({ id: null, name: "[Select]" } as ICustomerListItem);

    this.freightForwarders = _.cloneDeep(state.userFilteredCustomers.filter(c => c.filter === CustomerGroup.LogisticsCompany));
    this.freightForwarders.unshift({ id: null, name: "[Select]" } as ICustomerListItem);

    this.warehouses = _.cloneDeep(state.userFilteredCustomers.filter(c => c.filter === CustomerGroup.Warehouse));
    this.warehouses.unshift({ id: null, name: "[Select]" } as ICustomerListItem);

    this.customers = _.cloneDeep(state.userFilteredCustomers.filter(c => c.filter === CustomerGroup.BillLading));
    this.customers.unshift({ id: null, name: "[Select]" } as ICustomerListItem);

    this.ports = _.cloneDeep(state.port.list);
    this.ports.unshift({ id: null, name: "[Select]" } as IPort);
  }

  protected async activate(params: IParamsId) {
    await this.packingListService.getNonAllocatedContainers();
    await this.portService.loadPortList();

    if (params?.id) {
      await this.packingListService.load(params.id);
    } else {
      await this.packingListService.createPackingList();
    }
  }

  protected async deactivate() {
    await this.save();
  }

  protected get canSave() {
    return this.model?.containerIds.length > 0 && this.model?.bookingNumber?.length > 0 &&
      this.model?.contractNumber?.length > 0 && this.model?.vesselName?.length > 0 && this.model?.customerId &&
      this.model?.warehouseId && this.model?.destinationId && this.model?.shipperId &&
      this.model?.freightForwarderId;
  }

  protected get canDelete() {
    return this.model?.id !== undefined;
  }

  protected get caption() {
    if (this.model?.id) {
      return `Packing List #${getRavenRootId(this.model.id)}`;
    } else {
      return "New Packing List";
    }
  }

  protected async save() {
    if (this.canSave) {
      await this.packingListService.save(this.model);
      await this.packingListService.getNonAllocatedContainers();
    }
  }

  protected async delete() {
    this.dialogService.open({
      viewModel: DeleteDialog,
      model: { header: "Delete Packing List", body: "Are you sure you wish to delete this packing list?" }
    }).whenClosed(async result => {
      if (!result.wasCancelled) {
        await this.packingListService.deletePackingList(this.model.id);
        this.router.navigateToRoute("packingListList");
      }
    });
  }

  protected addContainer(container: IContainer) {
    this.model.containerIds.push(container.id);
    this.model.containers.push(container);
    container.packingListId = this.model.id;

    const containerIdx = this.unallocatedContainers.findIndex(c => c === container);
    if (containerIdx > -1) {
      this.unallocatedContainers.splice(containerIdx, 1);
    }
  }

  protected removeContainer(container: IContainer) {
    this.unallocatedContainers.push(container);
    container.packingListId = undefined;

    const containerIdx = this.model.containerIds.findIndex(c => c === container.id);
    if (containerIdx > -1) {
      this.model.containerIds.splice(containerIdx, 1);
    }

    const modelContainerIdx = this.model.containers.findIndex(c => c.id === container.id);
    if (modelContainerIdx > -1) {
      this.model.containers[modelContainerIdx].packingListId = undefined;
      this.model.containers.splice(modelContainerIdx, 1);
    }
  }

  protected async addPackingList() {
    this.router.navigateToRoute("packingListEdit", { id: null });
  }

  protected printPackingList() {
    this.router.navigateToRoute("packingListPrint", { id: encodeParams(this.model.id) });
  }  
}
