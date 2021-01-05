import { DialogService } from "aurelia-dialog";
import { autoinject, observable } from "aurelia-framework";
import { Router } from "aurelia-router";
import { connectTo } from "aurelia-store";
import { IPort } from "interfaces/IPort";
import { IEtaHistory } from "interfaces/shipping/IEtaHistory";
import { IVessel } from "interfaces/shipping/IVessel";
import _ from "lodash";
import moment from "moment";
import { VesselService } from "services/vessel-service";
import { IState } from "store/state";
import { DATEFORMAT } from "./../../constants/app-constants";
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
    if (this.model?.etaHistory.length === 0) {
      this.addEtaHistory();
    }
    this.customerList = _.cloneDeep(state.userFilteredCustomers);
    this.customerList.unshift({ id: null, name: "[Select]" } as ICustomerListItem);

    this.portsList = _.cloneDeep(state.port.list);
    this.portsList.unshift({ id: null, name: "[Select]" } as IPort);
  }

  protected get canSave() {
    return this.model?.etaHistory.length > 0 && this.model?.etaHistory[0]?.vesselName?.length > 0;
  }

  protected async save() {
    if (this.canSave) {
      await this.vesselService.save(this.model);
    }
  }

  protected addEtaHistory() {
    this.model.etaHistory.push({ dateUpdated: moment().format(DATEFORMAT) } as IEtaHistory);
  }

  protected async addBillLading() {
    await this.save();
    this.router.navigateToRoute("billLadingEdit",{vesselId: encodeParams(this.model.id), billLadingId: null});
  }
}
