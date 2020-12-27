import { IAppUserListItem } from "./IAppUser";
import { IEntity, IListItem } from "./IEntity";
export interface ICompany extends IEntity {
  users: string[];
  userDetails: IAppUserListItem[];
  contact: IContact;
  address: IAddress;
  taxId: string;
  activeSubscription: boolean;
  created: string;
  id: string;
  name: string;
  accessCode: number;
  demoMode: boolean;
}

export interface ICompanyListItem extends IListItem {
  activeSubscription: boolean;
  demoMode: boolean;
  created: string;
  contact: IContact;
}

export interface IAddress {
  street1: string;
  street2: string;
  city: string;
  postCode: string;
  country: string;
}

export interface IContact {
  name: string;
  email: string;
  tel: string;
}
