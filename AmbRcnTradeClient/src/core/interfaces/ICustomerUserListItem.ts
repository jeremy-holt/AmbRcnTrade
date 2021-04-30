import { IUser } from "../interfaces/IUser";

export interface ICustomerUserListItem {
    customerId: string;
    customerName: string;
    name: string;
    users: IUser[];
}
