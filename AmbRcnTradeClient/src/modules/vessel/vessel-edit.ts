import { DeleteDialog } from "dialogs/delete-dialog";
import { isInRole } from "core/services/role-service";
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
import { ICustomerListItem } from "core/interfaces/ICustomerListItem";
import { IParamsId } from "core/interfaces/IParamsId";
import { CustomerService } from "../../core/services/customer-service";
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
      c.shipperName = c.shipperId ? this.customerList.find(x => x.id === c.shipperId)?.name : "";
      c.consigneeName = c.consigneeId ? this.customerList.find(x => x.id === c.consigneeId)?.name : "";
      c.notifyParty1Name = c.notifyParty1Id ? this.customerList.find(x => x.id === c.notifyParty1Id)?.name : "";
    });
  }

  protected get canSave() {
    return this.model.vesselName?.length > 3 && this.model.voyageNumber?.length > 0 && this.model.bookingNumber?.length > 0 && this.model.shippingCompanyId !== null;
  }

  protected async save() {
    if (this.canSave) {
      await this.vesselService.save(this.model);
      this.router.navigateToRoute("vesselEdit", { id: encodeParams(this.model.id) }, { trigger: false, replace: true });
    }
  }

  protected async addBillLading() {
    await this.save();
    this.router.navigateToRoute("billLadingEdit", { vesselId: encodeParams(this.model.id), billLadingId: null });
  }

  protected encode(value: string) {
    return encodeParams(value);
  }

  protected get canAddBillLading() {
    return isInRole(["admin", "user"], this.state) && this.model.id?.length > 0;
  }

  protected get canEditBillLading() {
    return isInRole(["admin", "user"], this.state);
  }

  protected get canDeleteVessel() {
    return this.model?.id?.length > 0;
  }

  protected deleteVessel() {
    this.dialogService.open({
      viewModel: DeleteDialog,
      model: {
        header: "Delete this vessel",
        body: "Are you sure you wish to delete this vessel?<br>Doing so will delete all the Bills of Lading on this vessel, and return any boarded containers<br>to the container pool with status set to Stuffing Complete"
      }
    }).whenClosed(async result => {
      if (!result.wasCancelled) {
        await this.vesselService.deleteVessel(this.model.id);
        this.router.navigateToRoute("vesselList");
      }
    });
  }

}
