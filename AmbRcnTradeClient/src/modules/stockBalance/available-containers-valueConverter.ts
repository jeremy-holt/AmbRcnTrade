import { IAvailableContainer } from "./../../interfaces/stockManagement/IAvailableContainerItem";
import { autoinject } from "aurelia-dependency-injection";

@autoinject
export class AvailableContainersValueConverter {
  public toView(list: IAvailableContainer[], warehouseId: string) {
    if (!warehouseId) {
      return list;
    }

    return list.filter(c=>c.warehouseId===warehouseId);
  }
}
