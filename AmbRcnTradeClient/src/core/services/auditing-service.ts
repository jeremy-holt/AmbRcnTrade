import { IAuditLog } from "core/interfaces/IAuditLog";
import { Router } from "aurelia-router";
import { HttpClient } from "aurelia-fetch-client";
import { autoinject } from "aurelia-framework";
import { FetchService } from "./fetch-service";
import { Store } from "aurelia-store";
import { IState } from "store/state";
import { noOpAction } from "./no-op-action";

@autoinject
export class AuditingService extends FetchService {
  constructor(
    http: HttpClient,
    router: Router,
    store: Store<IState>
  ) {
    super("api/auditing", http, store, router);
  }

  public async loadList(){
    return await super.getData<IAuditLog[]>([],"loadList");
  }

  public async clearLogs(days: number){
    return await super.post({days},"clearLogs",noOpAction);
  }
}
