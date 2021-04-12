import { autoinject, observable } from "aurelia-framework";
import { connectTo } from "aurelia-store";
import { IBillLading } from "interfaces/shipping/IBillLading";
import { VesselService } from "services/vessel-service";
import { IState } from "store/state";
import { ICustomer } from "../../interfaces/ICustomer";
import { IVessel } from "../../interfaces/shipping/IVessel";
import { BillLadingService } from "../../services/bill-lading-service";
import { CustomerService } from "../../services/customer-service";
import { PortService } from "../../services/port-service";

@autoinject
@connectTo()
export class PackingList {
  @observable public state: IState = undefined!;
  protected model: IBillLading = undefined;
  protected customersList: ICustomer[] = [];
  protected vessel: IVessel = undefined!;

  protected shipperAddress = "";
  protected consigneeAddress = "";
  protected notifyParty1Address = "";
  protected forwardingAgentAddress = "";

  constructor(
    private billLadingService: BillLadingService,
    private customerService: CustomerService,
    private portService: PortService,
    private vesselService: VesselService
  ) { }

  protected async activate(prms: { vesselId: string; billLadingId: string }) {
    await this.customerService.loadAllCustomers();
    await this.portService.loadPortList();

    if (prms && !prms.vesselId) {
      throw new Error("Cannot access the Bill of Lading without the vesselId");
    }

    if (prms.billLadingId) {
      await this.billLadingService.load(prms.billLadingId);
    }

    if (prms.vesselId) {
      await this.vesselService.load(prms.vesselId);
    }
  }

  protected stateChanged(state: IState) {
    this.customersList = state.customer.list;
    this.model = state.billLading.current;
    this.vessel = state.vessel.current;

    const shipper = this.customersList.find(c => c.id === this.model.shipperId);
    this.shipperAddress = CustomerService.Address(shipper);

    const consignee = this.customersList.find(c => c.id === this.model.consigneeId);
    this.consigneeAddress = CustomerService.Address(consignee);

    const notifyParty1 = this.customersList.find(c => c.id === this.model.notifyParty1Id);
    this.notifyParty1Address = CustomerService.Address(notifyParty1);

    const forwardingAgent = this.customersList.find(c => c.id === this.vessel.forwardingAgentId);
    this.forwardingAgentAddress = CustomerService.Address(forwardingAgent);
  }
}
