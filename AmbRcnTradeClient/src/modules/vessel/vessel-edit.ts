import { isInRole } from "./../../services/role-service";
import { DialogService } from "aurelia-dialog";
import { autoinject, observable } from "aurelia-framework";
import { Router } from "aurelia-router";
import { connectTo } from "aurelia-store";
import { IPort } from "interfaces/IPort";
import { IVessel } from "interfaces/shipping/IVessel";
import _ from "lodash";
import { VesselService } from "services/vessel-service";
import { IState } from "store/state";
import { encodeParams } from "./../../core/helpers";
import { ICustomerListItem } from "./../../interfaces/ICustomerListItem";
import { IParamsId } from "./../../interfaces/IParamsId";
import { CustomerService } from "./../../services/customer-service";
import { PortService } from "./../../services/port-service";

@autoinject
@connectTo()
export class VesselEdit {
  @observable public state: IState = undefined!
  protected model: IVessel = undefined!;
  protected portsList: IPort[] = [];
  protected customerList: ICustomerListItem[] = [];

  constructor(
    private vesselService: VesselService,
    private router: Router,
    private dialogService: DialogService,
    private customerService: CustomerService,
    private portService: PortService
  ) { }

  protected async activate(prms: IParamsId) {
    await this.portService.loadPortList();
    await this.customerService.loadCustomersForAppUserList();

    if (prms?.id) {
      await this.vesselService.load(prms?.id);
    } else {
      await this.vesselService.createVessel();
    }
  }

  protected stateChanged(state: IState): void {
    this.model = _.cloneDeep(state.vessel.current);
    this.customerList = _.cloneDeep(state.userFilteredCustomers);
    this.customerList.unshift({ id: null, name: "[Select]" } as ICustomerListItem);

    this.portsList = _.cloneDeep(state.port.list);
    this.portsList.unshift({ id: null, name: "[Select]" } as IPort);

    this.model.billLadings.forEach(c => {
      c.shipperName = this.customerList.find(x => x.id === c.shipperId)?.name;
      c.consigneeName = this.customerList.find(x => x.id === c.consigneeId)?.name;
      c.notifyParty1Name = this.customerList.find(x => x.id === c.notifyParty1Id)?.name;
    });
  }

  protected get canSave() {
    return this.model.vesselName?.length > 3;
  }

  protected async save() {
    if (this.canSave) {
      await this.vesselService.save(this.model);
    }
  }

  protected async addBillLading() {
    await this.save();
    this.router.navigateToRoute("billLadingEdit", { vesselId: encodeParams(this.model.id), billLadingId: null });
  }

  protected encode(value: string) {
    return encodeParams(value);
  }

  protected get canAddBillLading(){
    return isInRole(["admin","user"], this.state);
  }

}
