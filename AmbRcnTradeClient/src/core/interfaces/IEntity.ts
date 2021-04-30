import { ILanguage } from "./ILanguage";

export interface IIdentity {
  id: string | null;
}

export interface IEntity extends IIdentity {
  name: string | null;
}

export interface IEntityCompany extends IEntity {
  companyId: string;
}

export interface IEntityCompanyTranslate extends IEntityCompany, ILanguage { }

export interface IIdentity {
  id: string | null;
}

export interface IListItem {
  id: string | null;
  name: string;
  filter?:string | null;
}

export interface IEntitySorted extends IEntity{
  sortOrder: number;
  description: string;
}
