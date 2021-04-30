import { CustomerGroup } from "./../../constants/app-constants";
export interface ICustomerListItem {    
    id: string;
    name: string;
    companyId: string;
    filter: CustomerGroup | null;
}
