import { IContractNumberListItem } from "./../../interfaces/contract-interfaces/IContract";
import { autoinject } from "aurelia-framework";

@autoinject
export class ContractListNumberFormatValueConverter {
  public toView(source: IContractNumberListItem[]) {
    const formatContractNumber = (item: IContractNumberListItem) => {
      const contractNumber = `${item.id} ${item.name}`;

      if (item.sellerRef && item.buyerRef) {
        return `${contractNumber} (${item.sellerRef}/${item.buyerRef})`;
      }

      if (item.sellerRef) {
        return `${contractNumber} (${item.sellerRef})`;
      }

      return `${contractNumber} (${item.buyerRef})`;
    }

    // if (!source) {
    //   return "";
    // }
    const value = source.forEach(c => { c.name = formatContractNumber(c) });
    console.log(source);
    console.log(value);

    return value;

  }
}
