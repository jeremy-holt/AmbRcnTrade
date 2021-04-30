import { PortService } from "./../../services/port-service";
import { autoinject } from "aurelia-dependency-injection";
import { observable } from "aurelia-framework";
import { Router } from "aurelia-router";
import { connectTo } from "aurelia-store";
import { ICustomer } from "core/interfaces/ICustomer";
import { IParamsId } from "core/interfaces/IParamsId";
import { CustomerService } from "core/services/customer-service";
import { IState } from "store/state";
import { IPackingList } from "./../../interfaces/shipping/IPackingList";
import { PackingListService } from "./../../services/packing-list-service";
import { IPort } from "interfaces/IPort";

@autoinject
@connectTo()
export class PackingListPrint {
  @observable private state: IState = undefined!;
  private model: IPackingList = undefined!;
  private customers: ICustomer[] = [];
  private ports: IPort[] = [];

  constructor(
    private packingListService: PackingListService,
    private router: Router,
    private customerService: CustomerService,
    private portService: PortService
  ) { }

  protected async activate(params: IParamsId) {
    await this.customerService.loadAllCustomers();
    await this.portService.loadPortList();

    if (params?.id) {
      await this.packingListService.load(params.id);
    }
  }

  protected stateChanged(state: IState) {
    this.model = state.packingList.current;
    this.customers = state.customer.list;
    this.ports = state.port.list;
  }

  protected get exporter() {
    return this.customers?.find(c => c.id === this.model?.shipperId)?.companyName;
  }

  protected get freightForwarder() {
    return this.customers?.find(c => c.id === this.model?.freightForwarderId)?.companyName;
  }

  protected get warehouseName() {
    return this.customers?.find(c => c.id === this.model?.warehouseId)?.companyName;
  }

  protected get customerName() {
    return this.customers?.find(c => c.id === this.model?.customerId)?.companyName;
  }

  protected get destination() {
    const port = this.ports?.find(c => c.id === this.model?.destinationId); {
      if (port) {
        return { portName: port.name, country: port.country };
      } else {
        return { portName: "", country: "" };
      }
    }
  }
}
