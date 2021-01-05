import { IStockListItem } from "../../interfaces/stocks/IStockListItem";
export class LotNoFilterValueConverter {
  public toView(list: IStockListItem[], filter: string) {
    if (filter === "[All]") {
      return list;
    }

    return list.filter(c => `Lot no ${c.lotNo}` === filter);
  }
}
