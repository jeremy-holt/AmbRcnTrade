import { PortService } from "../../../services/port-service";
import { observable, BindingEngine } from "aurelia-binding";
import { Subscription } from "aurelia-event-aggregator";
import { autoinject } from "aurelia-framework";
import { connectTo } from "aurelia-store";
import { decodeParams } from "core/helpers";
import { IParamsId } from "interfaces/IParamsId";
import { IPort } from "interfaces/IPort";
import _ from "lodash";
import { IState } from "store/state";

@autoinject
@connectTo()
export class PortEdit {
  @observable public state: IState = undefined!;
  public model: IPort = undefined!;
  public list: IPort[] = [];

  @observable public selectedPort: IPort = undefined!;

  private subscriptions: Subscription[] = [];

  constructor(
    private bindingEngine: BindingEngine,
    private portService: PortService
  ) { }

  protected stateChanged(state: IState) {
    this.model = state.port.current;
    this.list = _.cloneDeep(state.port.list);
  }

  protected async activate(params: IParamsId) {
    await this.portService.loadPortList();

    if (params.id) {
      await this.portService.loadPort(decodeParams(params.id) as string);
    }
  }

  protected bind() {
    this.subscriptions.push(this.bindingEngine.expressionObserver(this, "model.name").subscribe(() => this.updateDescription()));
    this.subscriptions.push(this.bindingEngine.expressionObserver(this, "model.country").subscribe(() => this.updateDescription()));
  }

  protected unbind() {
    this.subscriptions.forEach(c => c.dispose());
  }

  protected selectedPortChanged(newValue: IPort) {
    this.model = newValue;
  }

  protected async save() {
    await this.portService.savePort(this.model);
    await this.portService.loadPortList();

    this.selectedPort = this.list.find(c => c.name === this.model.name) as IPort;
  }

  protected async addNewPort() {
    await this.portService.createPort();
    this.selectedPort = null!;
  }

  protected updateDescription() {
    if (this.model?.name && this.model?.country) {
      this.model.description = `${this.model.name}, ${this.model.country}`;
    }
  }

  protected itemSelected(event: CustomEvent) {
    this.selectedPort = event.detail as IPort;
  }
}
