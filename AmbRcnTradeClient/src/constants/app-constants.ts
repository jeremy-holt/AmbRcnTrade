import { ICurrency } from "../core/interfaces/ICurrency";


export const DATEFORMAT = "YYYY-MM-DD";

export enum Currency {
  USD = "usd",
  EUR = "eur",
  CFA = "cfa",
  GBP = "gbp"
}

export enum PackingUnit {
  Lb = "lb",
  Kg = "kg",
  Mt = "mt"
}

export enum Teu {
  Teu20 = "teu20",
  Teu40 = "teu40"
}

export interface ITeu {
  id: Teu, name: string
}

export const TEU_LIST: ITeu[] = [
  { id: Teu.Teu40, name: "TC 40'" },
  { id: Teu.Teu20, name: "TC 20'" }
];

export enum Approval {
  Approved = "approved",
  Rejected = "rejected",
}

export interface IApproval { id: Approval, name: string; }

export const APPROVAL_LIST = [
  { id: Approval.Approved, name: "Approved" },
  { id: Approval.Rejected, name: "Rejected" }
];

export enum ImageType {
  image = "image",
  pdf = "pdf",
  other = "other"
}

export enum CustomerGroup {
  BillLading = "billLading",
  Supplier = "supplier",
  Buyer = "buyer",
  LogisticsCompany = "logisticsCompany",
  Warehouse = "warehouse"
}

export interface ICustomerGroup { id: CustomerGroup, name: string; }

export const CUSTOMER_GROUPS = [
  { id: CustomerGroup.BillLading, name: "Bill of lading" },
  { id: CustomerGroup.Supplier, name: "Supplier" },
  { id: CustomerGroup.Buyer, name: "Buyer" },
  { id: CustomerGroup.LogisticsCompany, name: "Logistics company" },
  { id: CustomerGroup.Warehouse, name: "Warehouse" }
];

export enum InspectionFilters {
  cargoReady,
  cargoReadyWithInspection,
  cargoReadyWithoutInspection,
  cargoReadyMissingShippingInstructions
}

export const INSPECTION_FILTERS = [
  { id: null, name: "[All]" },
  { id: InspectionFilters.cargoReadyWithInspection, name: "Inspection booked" },
  { id: InspectionFilters.cargoReadyWithoutInspection, name: "Missing inspection booking" },
  { id: InspectionFilters.cargoReadyMissingShippingInstructions, name: "Cargo ready missing shipping instructions" }
];

export interface IInspectionFilter { id: InspectionFilters | null; name: string; }

export enum ContainerStatus {
  Empty = "empty",
  Stuffing = "stuffing",
  StuffingComplete = "stuffingComplete",
  OnWayToPort = "onWayToPort",
  Gated = "gated",
  OnBoardVessel = "onBoardVessel",
  Shipped = "shipped",
  Cancelled = "cancelled",
}

export interface IContainerStatus { id: ContainerStatus; name: string; }

export const CONTAINER_STATUS_LIST: IContainerStatus[] = [
  { id: ContainerStatus.Empty, name: "Empty" },
  { id: ContainerStatus.Stuffing, name: "Stuffing" },
  { id: ContainerStatus.StuffingComplete, name: "Stuffing complete" },
  { id: ContainerStatus.OnWayToPort, name: "On way to port" },
  { id: ContainerStatus.Gated, name: "Gated" },
  { id: ContainerStatus.OnBoardVessel, name: "On board vessel" },
  { id: ContainerStatus.Shipped, name: "Shipped" },
  { id: ContainerStatus.Cancelled, name: "Cancelled" }
];

export enum PriceUnit {
  PerLb = "perLb",
  PerKg = "perKg",
  PerMt = "perMt",
}


export enum ShipmentOption {
  SellersOption = "sellersOption",
  BuyersCall = "buyersCall"
}

export interface IStockBalanceFilterItem { id: StockBalanceFilter, name: string }

export enum StockBalanceFilter {
  WithStockBalance = 2,
  NoStocks = 3
}

export const STOCK_BALANCE_FILTER_LIST:IStockBalanceFilterItem[] = [
  { id: null, name: "[All]" },
  { id: StockBalanceFilter.WithStockBalance, name: "With stock balance" },
  { id: StockBalanceFilter.NoStocks, name: "No stocks" }
];


export const CURRENCIES_LIST: ICurrency[] = [
  { id: Currency.USD, name: "USD" },
  { id: Currency.EUR, name: "EUR" },
  { id: Currency.CFA, name: "CFA" },
  { id: Currency.GBP, name: "GBP" }
];
