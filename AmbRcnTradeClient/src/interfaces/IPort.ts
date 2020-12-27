import { IEntityCompany } from "./IEntity";

export interface IPort extends IEntityCompany
{
    id: string | null;
    name: string;
    country: string;
    companyId: string;
    description: string;
}
