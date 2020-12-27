import { IInvoiceType } from "interfaces/contract-interfaces/IInvoiceType";
import { ICurrency } from "../interfaces/ICurrency";
import { IPackingUnit } from "../interfaces/IPackingUnit";
import { IPriceUnit } from "../interfaces/IPriceUnit";
import { IShipmentOption } from "../interfaces/IShipmentOption";

export const DATEFORMAT = "YYYY-MM-DD";

export enum Currency {
  USD = "usd",
  EUR = "eur",
  CFA = "cfa",
  GBP = "gbp"
}

export enum InvoiceType {
  D = "D",
  C = "C"
}

export enum PackingUnit {
  Lb = "lb",
  Kg = "kg",
  Mt = "mt"
}

export enum ImageType {
  image = "image",
  pdf = "pdf",
  other = "other"
}

export const GRADE_BASE_TYPES = [
  "",
  "WW",
  "SW",
  "SSW",
  "DW",
  "PWK1",
  "PWK2",
  "PWK3",
  "FS",
  "FB",
  "SS",
  "SB",
  "LWP",
  "LP",
  "SP",
  "SWP",
  "CH",
  "BB",
  "Meal",
  "Part peeled",
  "Unpeeled"
];

export const PACKING_UNITS: IPackingUnit[] = [
  { id: PackingUnit.Lb, name: "lb" },
  { id: PackingUnit.Kg, name: "kg" },
  { id: PackingUnit.Mt, name: "mt" }
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

export enum ContractSortOrder {
  period = "Period",
  id = "Id"
}

export enum KernelSize {
  wholes = "wholes",
  pieces = "pieces"
}

export const KERNEL_SIZE_LIST: IKernelSize[] = [
  { id: KernelSize.wholes, name: "Wholes" },
  { id: KernelSize.pieces, name: "Pieces" }
];


export enum KernelColor {
  first = "first",
  second = "second",
  third = "third",
  fourth = "fourth",
  dessert = "dessert",
  partPeeled = "partPeeled",
  unpeeled = "unpeeled"
}

export const KERNEL_COLOR_LIST: IKernelColor[] = [
  { id: KernelColor.first, name: "First" },
  { id: KernelColor.second, name: "Second" },
  { id: KernelColor.third, name: "Third" },
  { id: KernelColor.fourth, name: "Fourth" },
  { id: KernelColor.dessert, name: "Dessert" },
  { id: KernelColor.partPeeled, name: "Part peeled" },
  { id: KernelColor.unpeeled, name: "Unpeeled" }
];

export interface IKernelSize { id: KernelSize; name: string; }
export interface IKernelColor { id: KernelColor; name: string; }

export enum CalculationType {
  pct = 1,
  direct,
  total,
  max
}

export const CALCULATION_TYPES_LIST: ICalculationType[] = [
  { id: CalculationType.pct, name: "%" },
  { id: CalculationType.direct, name: "Direct" },
  { id: CalculationType.total, name: "Total" },
  { id: CalculationType.max, name: "Max" }
];

export interface ICalculationType { id: CalculationType, name: string; }

export enum DefectGroup {
  seriousDamage = 1,
  defects,
  breakage,
  info,
  roastTest
}

export const DEFECT_GROUPS_LIST: IDefectGroup[] = [
  { id: DefectGroup.seriousDamage, name: "Serious damage" },
  { id: DefectGroup.defects, name: "Defects" },
  { id: DefectGroup.breakage, name: "Breakage" },
  { id: DefectGroup.info, name: "Info" },
  { id: DefectGroup.roastTest, name: "Roast test" }
];

export interface IDefectGroup { id: DefectGroup; name: string; }

export const CONTRACT_SORTORDER_LIST = [
  ContractSortOrder.period,
  ContractSortOrder.id
];

// export const CONTAINER_STATUS_LIST = [
//   ContainerStatus.Open,
//   ContainerStatus.Shipped,
//   ContainerStatus.Cancelled,
//   ContainerStatus.ShippedWithoutDocuments
// ];

export enum PriceUnit {
  PerLb = "perLb",
  PerKg = "perKg",
  PerMt = "perMt",
}

export const PRICE_UNITS: IPriceUnit[] = [
  { id: PriceUnit.PerLb, name: "per lb" },
  { id: PriceUnit.PerKg, name: "per kg" },
  { id: PriceUnit.PerMt, name: "per mt" }
];

export enum ShipmentOption {
  SellersOption = "sellersOption",
  BuyersCall = "buyersCall"
}

export const SHIPMENT_OPTIONS: IShipmentOption[] = [
  { id: ShipmentOption.SellersOption, name: "Seller's option" },
  { id: ShipmentOption.BuyersCall, name: "Buyer's call" }
];

export const CURRENCIES_LIST: ICurrency[] = [
  { id: Currency.USD, name: "USD" },
  { id: Currency.EUR, name: "EUR" },
  { id: Currency.CFA, name: "CFA" },
  { id: Currency.GBP, name: "GBP" }
];

export const INVOICE_TYPES_LIST: IInvoiceType[] = [
  { id: InvoiceType.D, name: "D" },
  { id: InvoiceType.C, name: "C" }
];

export enum AccountType {
  assetsOtherAsset = "assetsOtherAsset",
  assetsOtherCurrentAssets = "assetsOtherCurrentAssets",
  assetsCash = "assetsCash",
  assetsBank = "assetsBank",
  assetsFixedAssets = "assetsFixedAssets",
  assetsStock = "assetsStock",
  assetsAccountsReceivable = "assetsAccountsReceivable",
  assetsPaymentClearing = "assetsPaymentClearing",
  liabilityOtherCurrentLiability = "liabilityOtherCurrentLiability",
  liabilityCreditCard = "liabilityCreditCard",
  liabilityLongTermLiability = "liabilityLongTermLiability",
  liabilityOtherLiability = "liabilityOtherLiability",
  liabilityOverseasTaxPayable = "liabilityOverseasTaxPayable",
  liabilityAccountsPayable = "liabilityAccountsPayable",
  equity = "equity",
  income = "income",
  incomeSales = "incomeSales",
  incomeOtherIncome = "incomeOtherIncome",
  expense = "expense",
  expenseCostOfGoodsSold = "expenseCostOfGoodsSold",
  expenseOtherExpense = "expenseOtherExpense",
  clients = "clients"
}

export enum AccountGroup {
  assets = "assets",
  liability = "liability",
  equity = "equity",
  income = "income",
  expense = "expense",
  clients = "clients"
}

export enum CommissionReportType {
  month,
  seller,
  buyer
}

export interface IAccountGroupListItem {
  id: AccountGroup; name: string;
}

export const ACCOUNT_GROUP_LIST: IAccountGroupListItem[] = [
  { id: AccountGroup.assets, name: "Assets" },
  { id: AccountGroup.liability, name: "Liability" },
  { id: AccountGroup.equity, name: "Equity" },
  { id: AccountGroup.income, name: "Income" },
  { id: AccountGroup.expense, name: "Expense" },
  { id: AccountGroup.clients, name: "Clients" }
];

export interface IAccountTypeListItem {
  id: AccountType;
  accountGroup: AccountGroup;
  name: string;
}

export const ACCOUNT_TYPES_LIST: IAccountTypeListItem[] = [
  { id: AccountType.assetsOtherAsset, accountGroup: AccountGroup.assets, name: "Other Asset" },
  { id: AccountType.assetsOtherCurrentAssets, accountGroup: AccountGroup.assets, name: "Other Current Assets" },
  { id: AccountType.assetsCash, accountGroup: AccountGroup.assets, name: "Cash" },
  { id: AccountType.assetsBank, accountGroup: AccountGroup.assets, name: "Bank" },
  { id: AccountType.assetsFixedAssets, accountGroup: AccountGroup.assets, name: "Fixed Assets" },
  { id: AccountType.assetsStock, accountGroup: AccountGroup.assets, name: "Stock" },
  { id: AccountType.assetsPaymentClearing, accountGroup: AccountGroup.assets, name: "Payment Clearing" },
  { id: AccountType.assetsAccountsReceivable, accountGroup: AccountGroup.assets, name: "Accounts Receivable" },
  { id: AccountType.liabilityOtherCurrentLiability, accountGroup: AccountGroup.liability, name: "Other Current Liability" },
  { id: AccountType.liabilityCreditCard, accountGroup: AccountGroup.liability, name: "Credit Card" },
  { id: AccountType.liabilityLongTermLiability, accountGroup: AccountGroup.liability, name: "Long Term Liability" },
  { id: AccountType.liabilityOtherLiability, accountGroup: AccountGroup.liability, name: "Other Liability" },
  { id: AccountType.liabilityOverseasTaxPayable, accountGroup: AccountGroup.liability, name: "Overseas Tax Payable" },
  { id: AccountType.liabilityAccountsPayable, accountGroup: AccountGroup.liability, name: "Accounts Payable" },
  { id: AccountType.equity, accountGroup: AccountGroup.equity, name: "Equity" },
  { id: AccountType.income, accountGroup: AccountGroup.income, name: "Income" },
  { id: AccountType.incomeSales, accountGroup: AccountGroup.income, name: "Sales Income" },
  { id: AccountType.incomeOtherIncome, accountGroup: AccountGroup.income, name: "Other Income" },
  { id: AccountType.expense, accountGroup: AccountGroup.expense, name: "Expense" },
  { id: AccountType.expenseCostOfGoodsSold, accountGroup: AccountGroup.expense, name: "Cost of Goods Sold" },
  { id: AccountType.expenseOtherExpense, accountGroup: AccountGroup.expense, name: "Other Expense" },
  { id: AccountType.clients, accountGroup: AccountGroup.clients, name: "Clients" }
];
