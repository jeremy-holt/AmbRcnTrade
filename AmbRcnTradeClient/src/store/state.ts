import { IBlendedStock } from "./../interfaces/stockManagement/IBlendedStock";
import { IPackingList } from "./../interfaces/shipping/IPackingList";
import { IBillLading } from "./../interfaces/shipping/IBillLading";
import { IVesselListItem } from "./../interfaces/shipping/IVesselListItem";
import { IAvailableContainer } from "./../interfaces/stockManagement/IAvailableContainerItem";
import { IOutgoingStock } from "./../interfaces/stockManagement/IIncomingStock";
import { IAppUser, IAppUserListItem } from "core/interfaces/IAppUser";
import { IAttachmentInfo } from "core/interfaces/IAttachmentInfo";
import { ICompany, ICompanyListItem } from "core/interfaces/ICompany";
import { ICustomer } from "core/interfaces/ICustomer";
import { ICustomerUserListItem } from "core/interfaces/ICustomerUserListItem";
import { IListItem } from "core/interfaces/IEntity";
import { IPayload } from "core/interfaces/IPayload";
import { IPort } from "interfaces/IPort";
import { IRoleNameItem } from "core/interfaces/IRoleNameItem";
import { IStock } from "interfaces/stocks/IStock";
import { IInspection } from "./../interfaces/inspections/IInspection";
import { IInspectionListItem } from "./../interfaces/inspections/IInspectionListItem";
import { IPurchase } from "./../interfaces/purchases/IPurchase";
import { IPurchaseListItem } from "./../interfaces/purchases/IPurchaseListItem";
import { IStockBalance } from "./../interfaces/stocks/IStockBalance";
import { IStockListItem } from "./../interfaces/stocks/IStockListItem";
import { IContainer } from "interfaces/shipping/IContainer";
import { IVessel } from "interfaces/shipping/IVessel";
import { ICustomerListItem } from "core/interfaces/ICustomerListItem";
import { IBillLadingListItem } from "interfaces/shipping/IBillLadingListItem";
import { IPaymentListItem } from "interfaces/payments/IPaymentListItem";
import { IPaymentDto } from "interfaces/payments/IPaymentDto";
import { IPayment } from "interfaces/payments/IPayment";

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
  stockManagement: { stuffContainer: IOutgoingStock[], availableContainers: IAvailableContainer[], blendedStock: IBlendedStock };
  container: { current: IContainer; list: IContainer[] };
  vessel: { current: IVessel; list: IVesselListItem[]; notLoadedContainers: IContainer[]; }
  billLading: {current: IBillLading; list: IBillLadingListItem[];}  
  payment: {current: IPayment, paymentDto: IPaymentDto, list: IPaymentListItem[]}
  packingList: {current: IPackingList; list: IPackingList[]; unallocatedContainers:IContainer[]}
}
