import { FetchService } from "./fetch-service";
import { autoinject } from "aurelia-framework";
import { HttpClient } from "aurelia-fetch-client";
import { Router } from "aurelia-router";
import { Store } from "aurelia-store";
import { IState } from "store/state";
import _ from "lodash";
import { IPort } from "interfaces/IPort";
import { QueryId } from "models/QueryId";

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
    return super.get(id, "load", portEditAction);
  }

  public async savePort(model: IPort) {
    return super.post(model, "save", portEditAction);
  }

  public async loadPortList() {
    return super.getMany<IPort[]>([new QueryId("companyId", super.getStateCurrentCompanyId)], "loadPorts", portListAction);
  }

  public async createPort() {
    return super.get([], "create", portEditAction);
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
