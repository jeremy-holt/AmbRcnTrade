import { DialogService } from "aurelia-dialog";
import { autoinject, observable } from "aurelia-framework";
import { Router } from "aurelia-router";
import { connectTo } from "aurelia-store";
import { IVessel } from "interfaces/shipping/IVessel";
import _ from "lodash";
import { BillLadingUploadDialog } from "modules/billLadingUploadDialog/billLading-upload-dialog";
import { DocumentsDownloadDialog } from "modules/documents-download-dialog/documents-download-dialog";
import { IState } from "store/state";
import { ITeu, TEU_LIST } from "./../../constants/app-constants";
import { encodeParams } from "./../../core/helpers";
import { DeleteDialog } from "./../../dialogs/delete-dialog";
import { ICustomerListItem } from "./../../interfaces/ICustomerListItem";
import { IPort } from "./../../interfaces/IPort";
import { IBillLading } from "./../../interfaces/shipping/IBillLading";
import { IContainer } from "./../../interfaces/shipping/IContainer";
import { BillLadingService } from "./../../services/bill-lading-service";
import { CustomerService } from "./../../services/customer-service";
import { PortService } from "./../../services/port-service";
import { isInRole } from "./../../services/role-service";
import { VesselService } from "./../../services/vessel-service";
import { AddContainersDialog } from "./add-containers-dialog";
import { MoveBillLadingDialog } from "./move-billLading-dialog";

@autoinject
@connectTo()
export class BillLadingEdit {
  @observable protected state: IState = undefined!;
  protected model: IBillLading = undefined;
  protected vessel: IVessel = undefined;
  protected customersList: ICustomerListItem[] = [];
  protected portsList: IPort[] = [];
  protected teuList: ITeu[] = [];

  constructor(
    private billLadingService: BillLadingService,
    private vesselService: VesselService,
    private customerService: CustomerService,
    private dialogService: DialogService,
    private portService: PortService,
    private router: Router
  ) { }

  protected stateChanged(state: IState) {
    this.customersList = _.cloneDeep(state.userFilteredCustomers);
    this.customersList.unshift({ id: null, name: "[Select]" } as ICustomerListItem);

    this.portsList = _.cloneDeep(state.port.list);
    this.portsList.unshift({ id: null, name: "[Select]" } as IPort);

    this.model = _.cloneDeep(state.billLading.current);
    this.vessel = _.cloneDeep(state.vessel.current);

    this.teuList = _.cloneDeep(TEU_LIST);
    this.teuList.unshift({ id: null, name: "[Select]" });
  }

  protected async activate(prms: { vesselId: string; billLadingId: string }) {
    await this.customerService.loadCustomersForAppUserList();
    await this.portService.loadPortList();

    if (prms && !prms.vesselId) {
      throw new Error("Cannot access the Bill of Lading without the vesselId");
    }

    if (prms?.vesselId) {
      await this.vesselService.load(prms?.vesselId);
    }

    if (prms.billLadingId) {
      await this.billLadingService.load(prms.billLadingId);
    } else {
      await this.billLadingService.createBillLading(prms.vesselId);
    }

  }

  protected get canSave() {
    return this.canEditBillLading && this.model?.teu && this.model?.portOfLoadingId !== null && this.model?.portOfDestinationId !== null;
  }

  protected async save() {
    if (this.canSave) {
      await this.billLadingService.save(this.model);
      await this.billLadingService.load(this.model.id);
    }
  }

  protected get freightForwarderName() {
    return this.state ? this.customersList.find(c => c.id === this.vessel.forwardingAgentId)?.name : "";
  }

  protected encode(value: string) {
    return encodeParams(value);
  }

  protected removeContainer(index: number) {
    const billLadingId = this.model.id;

    this.dialogService.open({
      viewModel: DeleteDialog,
      model: {
        header: "Remove container from Bill of Lading",
        body: "This will remove the container from this Bill of Lading. It will not delete it.<br>The status of the container will be changed to <b>Gated</b>"
      }
    }).whenClosed(async result => {
      if (!result.wasCancelled) {
        const containerToRemove = this.model.containerIds[index];
        await this.billLadingService.removeContainersFromBillLading(billLadingId, [containerToRemove]);
        await this.billLadingService.load(billLadingId);
      }
    });
  }

  protected async addContainersDialog() {
    await this.billLadingService.getNotLoadedContainers();
    const vesselId = this.model.vesselId;

    await this.dialogService.open({
      viewModel: AddContainersDialog,
      model: { unloadedContainers: this.state.vessel.notLoadedContainers }
    })
      .whenClosed(async result => {
        if (!result.wasCancelled) {
          const selectedRows = result.output as IContainer[];
          selectedRows.forEach(row => {
            this.model.containerIds.push(row.id);
            this.model.containers.push(row);
          });
          await this.billLadingService.addContainersToBillLading(this.model.id, vesselId, this.model.containerIds);
          await this.billLadingService.load(this.model.id);
        }
      });
  }

  protected async uploadDocuments() {
    await this.dialogService.open({
      viewModel: BillLadingUploadDialog,
      model: { billLadingId: this.model.id }
    })
      .whenClosed(async () => await this.save()
      );
  }

  protected async downloadDocuments() {
    await this.dialogService.open({
      viewModel: DocumentsDownloadDialog,
      model: { billLadingId: this.model.id }
    }).whenClosed();
  }

  protected get canAddContainer() {
    return isInRole(["admin", "user"], this.state);
  }

  protected get canPrintPackingList() {
    return isInRole(["admin", "user"], this.state);
  }

  protected get canEditBillLading() {
    return isInRole(["admin", "user"], this.state);
  }

  protected get canPrintBillLading() {
    return this.model?.id?.length > 0 && this.model?.shipperId?.length > 0 && this.model?.consigneeId?.length > 0 && this.model?.portOfLoadingId?.length > 0 && this.model?.portOfDestinationId?.length > 0 && ((this.model.teu as unknown) as number) !== 0;
  }

  protected navigateToVessel() {
    this.router.navigateToRoute("vesselEdit", { id: encodeParams(this.vessel.id) });
  }

  protected async printBillLading() {
    await this.save();
    const objectUrl = await this.billLadingService.getDraftBillOfLading(this.vessel.id, this.model.id);
    window.location.href = objectUrl;
  }

  protected moveBillLadingToVessel() {
    this.dialogService.open({
      viewModel: MoveBillLadingDialog,
      model: {
        state: this.state,
        billLadingId: this.model.id,
        fromVesselId: this.vessel.id
      }
    }).whenClosed(async result => {
      if (!result.wasCancelled) {
        const { billLadingId, fromVesselId, toVesselId } = result.output;
        await this.billLadingService.moveBillLadingToVessel(billLadingId, fromVesselId, toVesselId);
        this.router.navigateToRoute("vesselEdit", { id: encodeParams(toVesselId) });
      }
    });
  }

  protected deleteBillLading() {
    const vesselId = this.vessel.id;
    const billLadingId = this.model.id;

    this.dialogService.open(
      {
        viewModel: DeleteDialog,
        model: {
          header: "Delete Bill of Lading",
          body: "Are you sure you wish to delete this Bill of Lading?<br>This will remove it from the vessel and set the status for all the containers to Gated"
        }
      }
    ).whenClosed(async result => {
      if (!result.wasCancelled) {
        await this.billLadingService.deleteBillLading(vesselId, billLadingId);
        this.router.navigateToRoute("vesselEdit", { id: encodeParams(this.vessel.id) });
      }
    });
  }
}
