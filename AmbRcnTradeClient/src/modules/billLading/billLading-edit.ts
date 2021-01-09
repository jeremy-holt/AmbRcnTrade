import { IVessel } from "interfaces/shipping/IVessel";
import { isInRole } from "./../../services/role-service";
import { DeleteDialog } from "./../../dialogs/delete-dialog";
import { encodeParams } from "./../../core/helpers";
import { DialogService } from "aurelia-dialog";
import { autoinject, observable } from "aurelia-framework";
import { Router } from "aurelia-router";
import { connectTo } from "aurelia-store";
import _ from "lodash";
import { IState } from "store/state";
import { ICustomerListItem } from "./../../interfaces/ICustomerListItem";
import { IBillLading } from "./../../interfaces/shipping/IBillLading";
import { IContainer } from "./../../interfaces/shipping/IContainer";
import { BillLadingService } from "./../../services/bill-lading-service";
import { CustomerService } from "./../../services/customer-service";
import { AddContainersDialog } from "./add-containers-dialog";
import { DocumentsDownloadDialog } from "modules/documents-download-dialog/documents-download-dialog";
import { BillLadingUploadDialog } from "modules/billLadingUploadDialog/billLading-upload-dialog";

@autoinject
@connectTo()
export class BillLadingEdit {
  @observable protected state: IState = undefined!;
  protected model: IBillLading = undefined;
  protected vessel: IVessel = undefined;
  protected customersList: ICustomerListItem[] = [];
  protected vesselId = "";

  constructor(
    private billLadingService: BillLadingService,
    private customerService: CustomerService,
    private dialogService: DialogService,
    private router: Router
  ) { }

  protected stateChanged(state: IState) {
    this.customersList = _.cloneDeep(state.userFilteredCustomers);
    this.customersList.unshift({ id: null, name: "[Select]" } as ICustomerListItem);

    this.model = state.billLading.current;
    this.vessel = state.vessel.current;
  }

  protected async activate(prms: { vesselId: string; billLadingId: string }) {
    await this.customerService.loadCustomersForAppUserList();

    if (prms && !prms.vesselId) {
      throw new Error("Cannot access the Bill of Lading without the vesselId");
    }

    if (prms.billLadingId) {
      await this.billLadingService.load(prms.billLadingId);
    } else {
      await this.billLadingService.createBillLading(prms.vesselId);
    }

    this.vesselId = prms?.vesselId;
  }

  protected get canSave() {
    return this.canEditBillLading;
  }

  protected async save() {
    if (this.canSave) {
      await this.billLadingService.save(this.model);
    }
  }

  protected get vesselName() {
    return this.state ? this.state.vessel.list.find(c => c.id === this.model.vesselId)?.vesselName : "";
  }

  protected encode(value: string) {
    return encodeParams(value);
  }

  protected removeContainer(index: number) {
    this.dialogService.open({
      viewModel: DeleteDialog,
      model: { body: "This will remove the container from this Bill of Lading. It will not delete the container" }
    }).whenClosed(async result => {
      if (!result.wasCancelled) {
        this.model.containerIds.splice(index, 1);
        this.model.containers.splice(index, 1);
        await this.save();
      }
    });
  }

  protected async addContainersDialog() {
    await this.billLadingService.getNotLoadedContainers();
    this.dialogService.open({
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
          await this.save();
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

  protected get canEditBillLading(){
    return isInRole(["admin","user"],this.state);
  }
}
