import {IAddress} from "./ICompany";
import {IEntity, IListItem} from "./IEntity";
import {RoleType} from "./types";

export interface IAppUser extends IEntity {
  approved: boolean;
  role: RoleType[] | RoleType;
  password: string;
  email: string;
  companyName: string;
  firstName: string;
  lastName: string;
  companyDetails: ICompanyDetails;
  clientCompaniesList: IClientCompanyItem[];
}

export interface IAppUserListItem extends IListItem {
  role?: string;
  email: string;
  approved: boolean;
  companyName: string;
}

export interface ICompanyDetails {
  companyName: string;
  address: IAddress;
  tel: string;
  taxId: string;
  notes: string;
}

export interface IClientCompanyItem {
  companyId: string;
  clientId: string;
}

