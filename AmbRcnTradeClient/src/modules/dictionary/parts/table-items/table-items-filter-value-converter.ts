import { IEntity } from "core/interfaces/IEntity";

export class TableItemsFilterValueConverter {
  public toView(list: IEntity[]) {
    return list.filter(c=>c.id !== null);
  }
}
