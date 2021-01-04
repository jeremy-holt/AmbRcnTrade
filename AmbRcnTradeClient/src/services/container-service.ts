import { fixAspNetCoreDate } from "./../core/helpers";
import { HttpClient } from "aurelia-fetch-client";
import { autoinject } from "aurelia-framework";
import { Router } from "aurelia-router";
import { Store } from "aurelia-store";
import { IUnstuffContainerRequest } from "interfaces/shipping/IUnstuffContainerRequest";
import _ from "lodash";
import { QueryId } from "models/QueryId";
import { IState } from "store/state";
import { ContainerStatus } from "./../constants/app-constants";
import { IContainer } from "./../interfaces/shipping/IContainer";
import { FetchService } from "./fetch-service";
import { noOpAction } from "./no-op-action";

@autoinject
export class ContainerService extends FetchService {
  constructor(
    http: HttpClient,
    store: Store<IState>,
    router: Router
  ) {
    super("api/container", http, store, router);

    store.registerAction("containerEditAction", containerEditAction);
    store.registerAction("containerListAction", containerListAction);
  }

  public async load(id: string) {
    return super.get(id, "load", containerEditAction);
  }

  public async loadList(status: ContainerStatus) {
    return super.getMany<IContainer[]>([super.currentCompanyIdQuery(), new QueryId("status", status)], "loadList", containerListAction);
  }

  public async save(container: IContainer) {
    return super.post(container, "save", containerEditAction);
  }

  public async createContainer() {
    return super.get([super.currentCompanyIdQuery()], "create", containerEditAction);
  }

  public async unstuffContainer(request: IUnstuffContainerRequest) {
    return super.post(request, "unstuffContainer", noOpAction);
  }

  public canUnstuffContainer(container: IContainer) {
    return [ContainerStatus.Cancelled, ContainerStatus.Empty, ContainerStatus.Stuffing, ContainerStatus.StuffingComplete].includes(container?.status) && container?.bags > 0;
  }

  public static isOverweightContainer(quantities: { bags: number, weightKg: number }) {
    return quantities.weightKg > 26_000 || quantities.bags > 325;
  }
}

export function containerEditAction(state: IState, container: IContainer) {
  container.dispatchDate = fixAspNetCoreDate(container.dispatchDate, false);
  container.stuffingDate = fixAspNetCoreDate(container.stuffingDate, false);
  container.incomingStocks.forEach(c => {
    c.stuffingDate = fixAspNetCoreDate(c.stuffingDate, false);
  });

  const newState = _.cloneDeep(state);
  newState.container.current = container;
  return newState;
}

export function containerListAction(state: IState, containers: IContainer[]) {
  const newState = _.cloneDeep(state);
  newState.container.list = containers;
  return newState;
}
