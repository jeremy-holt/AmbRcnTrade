import { CustomerGroup as CustomerGroup } from "./../constants/app-constants";
import { IUser } from "interfaces/IUser";
import { IEntityCompany } from "./IEntity";


export interface ICustomer extends IEntityCompany {
  id: string | null;
  name: string;
  companyId: string;
  companyName: string;
  address: IAddress | null;
  notes?: string;  
  users: IUser[];
  filter: CustomerGroup;
}

export interface IAddress {
  street1?: string | null;
  street2?: string | null;
  city?: string | null;
  postCode?: string | null;
  state?: string | null;
  country: string;
}
