import { ICurrency } from "../interfaces/ICurrency";


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
  All = "all",
  Open = "open",
  Shipped = "shipped",
  Cancelled = "cancelled",
  ShippedWithoutDocuments = "shippedWithoutDocuments"
}

export interface IContainerStatus { id: ContainerStatus; name: string; }
export const CONTAINER_STATUS_LIST: IContainerStatus[] = [
  { id: ContainerStatus.Open, name: "Open" },
  { id: ContainerStatus.Shipped, name: "Shipped" },
  { id: ContainerStatus.ShippedWithoutDocuments, name: "Shipped without documents" },
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


export const CURRENCIES_LIST: ICurrency[] = [
  { id: Currency.USD, name: "USD" },
  { id: Currency.EUR, name: "EUR" },
  { id: Currency.CFA, name: "CFA" },
  { id: Currency.GBP, name: "GBP" }
];
