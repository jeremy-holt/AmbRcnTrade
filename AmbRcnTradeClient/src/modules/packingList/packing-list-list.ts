import { encodeParams, getRavenRootId } from "./../../core/helpers";
import { Router } from "aurelia-router";
import { IPackingList } from "./../../interfaces/shipping/IPackingList";
import { PackingListService } from "./../../services/packing-list-service";
import { autoinject } from "aurelia-dependency-injection";
import { connectTo } from "aurelia-store";
import { IState } from "store/state";
import _ from "lodash";

@autoinject
@connectTo()
export class PackingListList {
  protected state: IState = undefined!;
  protected list: IPackingList[] = [];

  constructor(
    private packingListService: PackingListService,
    private router: Router
  ) { }

  protected stateChanged(state: IState) {
    this.list = _.cloneDeep(state.packingList.list);
  }

  protected async activate() {
    await this.packingListService.loadList();
  }

  protected async addPackingList() {
    this.router.navigateToRoute("packingListEdit");
  }

  protected getRavenRootId(id: string){
    return getRavenRootId(id);
  }

  protected encode(value: string){
    return encodeParams(value);
  }
}
