import { VesselService } from "./../../services/vessel-service";
import { autoinject, observable } from "aurelia-framework";
import { Router } from "aurelia-router";
import { connectTo } from "aurelia-store";
import { IState } from "store/state";
import { ICustomerListItem } from "./../../interfaces/ICustomerListItem";
import { IBillLading } from "./../../interfaces/shipping/IBillLading";
import { BillLadingService } from "./../../services/bill-lading-service";
import { CustomerService } from "./../../services/customer-service";

@autoinject
@connectTo()
export class BillLadingEdit {
  @observable protected state: IState = undefined!;
  protected model: IBillLading = undefined;
  protected customersList: ICustomerListItem[] = [];

  constructor(
    private billLadingService: BillLadingService,
    private customerService: CustomerService,
    private vesselService: VesselService,
    private router: Router
  ) { }

  protected stateChanged(state: IState) {
    this.customersList = state.userFilteredCustomers;
    this.model = state.billLading.current;
  }

  protected async activate(prms: { vesselId: string; billLadingId: string }) {
    await this.customerService.loadCustomersForAppUserList();
    await this.vesselService.loadList();

    if (prms && !prms.vesselId) {
      throw new Error("Cannot access the Bill of Lading without the vesselId");
    }

    if (prms.billLadingId) {
      await this.billLadingService.load(prms.billLadingId);
    } else {
      await this.billLadingService.createBillLading(prms.vesselId);
    }
  }

  protected get canSave() {
    return true;
  }

  protected async save() {
    if (this.canSave) {
      await this.billLadingService.save(this.model);
    }
  }

  protected get vesselName() {
    return this.state ? this.state.vessel.list.find(c => c.id === this.model.vesselId)?.vesselName : "";
  }
}
