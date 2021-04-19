import { IStockBalanceFilterItem, StockBalanceFilter } from "constants/app-constants";
import { IStockListItem } from "../../interfaces/stocks/IStockListItem";
export class LotNoFilterValueConverter {
  public toView(list: IStockListItem[], lotNo: number, stocksFilter: IStockBalanceFilterItem) {
    if (lotNo === null && stocksFilter?.id === null) {
      return list;
    }

    if (lotNo !== null) {
      return list.filter(c => c.lotNo === lotNo);
    }

    const groupIndexesWithStocks = list.filter(c => c.origin === "Stock balance" && c.bagsIn !== 0).map(c => c.lotNo);
    const groupIndexesWithoutStocks = list.filter(c => c.origin === "Stock balance" && c.bagsIn === 0).map(c => c.lotNo);

    switch (stocksFilter?.id) {
      case StockBalanceFilter.WithStockBalance:
        return list.filter(c => groupIndexesWithStocks.includes(c.lotNo));
      case StockBalanceFilter.NoStocks:
        return list.filter(c => groupIndexesWithoutStocks.includes(c.lotNo));
    }
  }
}
