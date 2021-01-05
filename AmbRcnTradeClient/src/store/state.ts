import { IBillLading } from "./../interfaces/shipping/IBillLading";
import { INotLoadedContainer } from "./../interfaces/shipping/INotLoadedContainer";
import { IVesselListItem } from "./../interfaces/shipping/IVesselListItem";
import { IAvailableContainer } from "./../interfaces/stockManagement/IAvailableContainerItem";
import { IOutgoingStock } from "./../interfaces/stockManagement/IIncomingStock";
import { IAppUser, IAppUserListItem } from "interfaces/IAppUser";
import { IAttachmentInfo } from "interfaces/IAttachmentInfo";
import { ICompany, ICompanyListItem } from "interfaces/ICompany";
import { ICustomer } from "interfaces/ICustomer";
import { ICustomerUserListItem } from "interfaces/ICustomerUserListItem";
import { IListItem } from "interfaces/IEntity";
import { IPayload } from "interfaces/IPayload";
import { IPort } from "interfaces/IPort";
import { IRoleNameItem } from "interfaces/IRoleNameItem";
import { IStock } from "interfaces/stocks/IStock";
import { IInspection } from "./../interfaces/inspections/IInspection";
import { IInspectionListItem } from "./../interfaces/inspections/IInspectionListItem";
import { IPurchase } from "./../interfaces/purchases/IPurchase";
import { IPurchaseListItem } from "./../interfaces/purchases/IPurchaseListItem";
import { IStockBalance } from "./../interfaces/stocks/IStockBalance";
import { IStockListItem } from "./../interfaces/stocks/IStockListItem";
import { IContainer } from "interfaces/shipping/IContainer";
import { IVessel } from "interfaces/shipping/IVessel";
import { ICustomerListItem } from "interfaces/ICustomerListItem";
import { IBillLadingListItem } from "interfaces/shipping/IBillLadingListItem";

export interface IState {
  userFilteredCustomers: ICustomerListItem[];
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
  inspection: { current: IInspection, list: IInspectionListItem[], movedToStockId: string; };
  stock: { current: IStock, list: IStockListItem[], stockBalanceList: IStockBalance[] };
  purchase: { current: IPurchase, list: IPurchaseListItem[], nonCommittedStocksList: IStockListItem[] };
  stockManagement: { stuffContainer: IOutgoingStock[], availableContainers: IAvailableContainer[] };
  container: { current: IContainer; list: IContainer[] };
  vessel: { current: IVessel; list: IVesselListItem[]; notLoadedContainers: INotLoadedContainer[]; }
  billLading: {current: IBillLading; list: IBillLadingListItem[];}  
}
