import { autoinject } from "aurelia-framework";
import { connectTo } from "aurelia-store";
import { CustomerGroup } from "constants/app-constants";
import { ICustomerListItem } from "../../interfaces/ICustomerListItem";

@autoinject
@connectTo()
export class CustomerFilterValueConverter {
  public toView(list: ICustomerListItem[], filter: CustomerGroup) {    
    if (!filter) {
      return [];
    }

    return list.filter(c => c.filter === filter || !c.id);
  }
}
