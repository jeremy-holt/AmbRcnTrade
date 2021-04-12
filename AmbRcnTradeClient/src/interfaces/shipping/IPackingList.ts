import { IContainer } from "./IContainer";

export interface IPackingList {
    id: string;
    name: string;
    companyId: string;
    containerIds: string[];
    bookingNumber: string;
    date: string;
    notes: string;
    containers: IContainer[];
}
