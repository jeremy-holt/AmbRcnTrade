import { IContainer } from "./../../interfaces/shipping/IContainer";
import { AddContainersDialog } from "./add-containers-dialog";
import { DialogService } from "aurelia-dialog";
import { VesselService } from "./../../services/vessel-service";
import { autoinject, observable } from "aurelia-framework";
import { Router } from "aurelia-router";
import { connectTo } from "aurelia-store";
import { IState } from "store/state";
import { ICustomerListItem } from "./../../interfaces/ICustomerListItem";
import { IBillLading } from "./../../interfaces/shipping/IBillLading";
import { BillLadingService } from "./../../services/bill-lading-service";
import { CustomerService } from "./../../services/customer-service";
import _ from "lodash";
import { INotLoadedContainer } from "interfaces/shipping/INotLoadedContainer";

@autoinject
@connectTo()
export class BillLadingEdit {
  @observable protected state: IState = undefined!;
  protected model: IBillLading = undefined;
  protected customersList: ICustomerListItem[] = [];

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
  }

  protected async activate(prms: { vesselId: string; billLadingId: string }) {
    await this.customerService.loadCustomersForAppUserList();
    await this.billLadingService.getNotLoadedContainers();

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

  protected addContainersDialog() {
    this.dialogService.open({
      viewModel: AddContainersDialog,
      model: {unloadedContainers: this.state.vessel.notLoadedContainers}
    })
      .whenClosed(result => {
        if (!result.wasCancelled) {
          const selectedRows=result.output as IContainer[];
          selectedRows.forEach(row=>{
            this.model.containerIds.push(row.containerId);
            this.model.containers.push(row>);
          })
          console.log(selectedRows);
        }
      });
  }
}
