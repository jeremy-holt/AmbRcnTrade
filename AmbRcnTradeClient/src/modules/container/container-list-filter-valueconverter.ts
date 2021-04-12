import { IContainer } from "interfaces/shipping/IContainer";
export class ContainerListFilterValueConverter {
  public toView(list: IContainer[], warehouseId: string) {
    if (!warehouseId) {
      return list;
    }

    return list.filter(c => c.warehouseId === warehouseId);
  }
}
