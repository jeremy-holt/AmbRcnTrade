import { autoinject, bindable } from "aurelia-framework";
import { Router } from "aurelia-router";
import { encodeParams } from "core/helpers";
import _ from "lodash";
import { IEntitySorted } from "./../../../interfaces/IEntity";

@autoinject
export class TableItems {
  @bindable public list: IEntitySorted[] = [];
  @bindable public editRoute: string = undefined!;

  public displayList: IEntitySorted[] = [];
  public hasDescription = true;

  constructor(
    private router: Router,
    private el: Element
  ) { }

  protected bind() {
    this.list = _.cloneDeep(this.list.filter(c => c.id !== null));
    this.hasDescription = this.list.every(c => c.description !== undefined);
  }

  protected editItem(id: string) {
    this.router.navigateToRoute(this.editRoute, { id: id });
  }

  protected encode(route: string) {
    return encodeParams(route);
  }

  protected orderChanged() {
    this.list.forEach((c, idx) => {
      c.sortOrder = idx + 1;
    });
  }

  protected setCurrentItem(item: IEntitySorted) {
    this.el.dispatchEvent(new CustomEvent("change", {
      bubbles: true,
      detail: item
    }));
  }
}
