import { HttpClient } from "aurelia-fetch-client";
import { autoinject } from "aurelia-framework";
import { Router } from "aurelia-router";
import { Store } from "aurelia-store";
import { FetchService } from "core/services/fetch-service";
import { IPort } from "interfaces/IPort";
import _ from "lodash";
import { IState } from "store/state";
@autoinject
export class PortService extends FetchService {
  constructor(
    http: HttpClient,
    router: Router,
    store: Store<IState>
  ) {
    super("api/port", http, store, router);

    store.registerAction("portEditAction", portEditAction);
    store.registerAction("portListAction", portListAction);
  }

  public async loadPort(id: string) {
    return await super.get(id, "load", portEditAction);
  }

  public async savePort(model: IPort) {
    return await super.post(model, "save", portEditAction);
  }

  public async loadPortList() {
    return await super.getMany<IPort[]>([this.currentCompanyIdQuery()], "loadPorts", portListAction);
  }

  public async createPort() {
    return await super.get([], "create", portEditAction);
  }
}

export function portEditAction(state: IState, port: IPort) {
  const newState = _.cloneDeep(state);
  newState.port.current = port;
  return newState;
}

export function portListAction(state: IState, list: IPort[]) {
  const newState = _.cloneDeep(state);
  newState.port.list = list;
  return newState;
}
