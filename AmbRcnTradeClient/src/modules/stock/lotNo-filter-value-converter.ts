import { IStockListItem } from "../../interfaces/stocks/IStockListItem";
export class LotNoFilterValueConverter {
  public toView(list: IStockListItem[], lotNo: number) {
    if (lotNo === null) {
      return list;
    }

    return list.filter(c => c.lotNo === lotNo);
  }
}
