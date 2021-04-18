import { ICustomerListItem } from "./../../interfaces/ICustomerListItem";
import { IStockBalanceFilterItem, StockBalanceFilter } from "./../../constants/app-constants";
import { IStockBalance } from "./../../interfaces/stocks/IStockBalance";
import _ from "lodash";
export class StockBalanceFilterValueConverter {
  public toView(list: IStockBalance[], filter: IStockBalanceFilterItem, warehouse: ICustomerListItem) {
    let returnValue = _.cloneDeep(list);

    if(!filter?.id && !warehouse?.id){
      return returnValue;
    }

    switch (filter?.id) {
      case StockBalanceFilter.NoStocks:
        returnValue = list.filter(c => c.balance === 0);
        break;
      case StockBalanceFilter.WithStockBalance:
        returnValue = list.filter(c => c.balance !== 0);
        break;
    }

    if (warehouse?.id === null) {
      return returnValue;
    } else {
      return returnValue.filter(c => c.locationId === warehouse?.id);
    }
  }
}
