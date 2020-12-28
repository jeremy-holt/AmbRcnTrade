import { IAppUser, IAppUserListItem } from "interfaces/IAppUser";
import { IAttachmentInfo } from "interfaces/IAttachmentInfo";
import { ICompany, ICompanyListItem } from "interfaces/ICompany";
import { ICustomer } from "interfaces/ICustomer";
import { ICustomerUserListItem } from "interfaces/ICustomerUserListItem";
import { IListItem } from "interfaces/IEntity";
import { IPayload } from "interfaces/IPayload";
import { IPort } from "interfaces/IPort";
import { IRoleNameItem } from "interfaces/IRoleNameItem";

export interface IState {
  userFilteredCustomers: IListItem[];
  currentCompanyId: string;
  currentCompanyName: string;
  culture: string;
  loggedIn: boolean;
  user: IPayload;
  serverMessages: {
    errorMessage: string;
    message: string;
  };
  userCompanies: IListItem[];
  userCustomers: string[];
  admin: {
    company: {
      current: ICompany;
      list: ICompanyListItem[];
    };
    user: {
      current: IAppUser;
      list: IAppUserListItem[];
    };
    roleNames: IRoleNameItem[];
  };

  customer: { current: ICustomer; list: ICustomer[]; usersList: ICustomerUserListItem[] };
 
  port: { current: IPort, list: IPort[] };
  attachmentRoutes: IAttachmentInfo[];
}
