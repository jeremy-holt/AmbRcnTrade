import { IStockBalanceFilterItem, StockBalanceFilter } from "./../../constants/app-constants";
import { IStockBalance } from "./../../interfaces/stocks/IStockBalance";
export class StockBalanceFilterValueConverter {
  public toView(list: IStockBalance[], filter: IStockBalanceFilterItem) {  
    switch (filter?.id) {
      case null:
        return list;
      case StockBalanceFilter.NoStocks:
        return list.filter(c => c.balance === 0);
      default:
        return list.filter(c => c.balance !== 0);
    }
  }
}
