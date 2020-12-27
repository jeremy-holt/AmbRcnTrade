import { ValidationRules } from "aurelia-validation";
import moment from "moment";
import "moment/locale/fr";
import "moment/locale/pt";
import "moment/locale/vi";
import { ILogin } from "../interfaces/ILogin";
import { IAppUser } from "./../interfaces/IAppUser";
import { ICompany } from "./../interfaces/ICompany";

// export const client = ValidationRules
//   .ensure((c: IBaseClient) => c.name).required().withMessage("* Please provide the name of the company");

// export const destination = ValidationRules
//   .ensure((c: IDestination) => c.name).required().withMessage("* Please provide the name of the destination");

// export const region = ValidationRules
//   .ensure((c: IRegion) => c.name).required().withMessage("* Please provide the name of the region");

// export const grade = ValidationRules
//   .ensure((c: IGrade) => c.name).required().withMessage("* Please provide the name of the grade");

// export const paymentTerm = ValidationRules
//   .ensure((c: IPaymentTerm) => c.en).required().withMessage("* Please provide the name of the payment term in English")
//   .ensure((c: IPaymentTerm) => c.fr).required().withMessage("* Please provide the name of the payment term in French");

// export const incoterm = ValidationRules
//   .ensure((c: IIncoterm) => c.en).required().withMessage("* Please provide the name of the shipment basis in English")
//   .ensure((c: IIncoterm) => c.fr).required().withMessage("* Please provide the name of the shipment basis in French");

// export const language = ValidationRules
//   .ensure((c: ILanguage) => c.en).required().withMessage("* Please provide the name of in English")
//   .ensure((c: ILanguage) => c.fr).required().withMessage("* Please provide the name of in French");

// export const purchase = ValidationRules
//   .ensure((c: IPurchase) => c.deliveryDate).required().withMessage("* Please provide the delivery date")
//   .ensure((c: IPurchase) => c.kor).satisfies(val => val > 0).withMessage("* KOR must be greater than 0")
//   .ensure((c: IPurchase) => c.quantityKg).satisfies(val => val > 0).withMessage("* Quantity must be greater than 0")
//   .ensure((c: IPurchase) => c.price).satisfies(val => val > 0).withMessage("* Price must be greater than 0");

export const company = ValidationRules
  .ensure((c: ICompany) => c.name).required().withMessage("* Please provide the company name");

export const appUser = ValidationRules
  .ensure((c: IAppUser) => c.email).required().withMessage("* Please provide the email")
  .ensure((c: IAppUser) => c.firstName).required().withMessage("* Please provide the first name")
  .ensure((c: IAppUser) => c.lastName).required().withMessage("* Please provide the last name")
  .ensure((c: IAppUser) => c.role).required().withMessage("* Please provide the user's role(s)");

// export const arrival = ValidationRules
//   .ensure((c: IArrival) => c.quantityBags).satisfies(val => val > 0).withMessage("* Quantity must be greater than 0")
//   .ensure((c: IArrival) => c.quantityKg).satisfies(val => val > 0).withMessage("* Quantity must be greater than 0");

// export const inspection = ValidationRules
//   .ensure((c: IInspection) => c.inspectedBy).required().withMessage("* Please provide the name of the inspector");

// export const analysis = ValidationRules
//   .ensure((c: IAnalysis) => c.moisturePct).satisfies(val => val > 0).withMessage("* Moisture % must be greater than 0")
//   .ensure((c: IAnalysis) => c.count).satisfies(val => val > 0).withMessage("* Count must be greater than 0")
//   .ensure((c: IAnalysis) => c.kor).satisfies(val => val > 0).withMessage("* KOR must be greater than 0");

export const login = ValidationRules
  .ensure((c: ILogin) => c.email).required().withMessage("* Please provide your email")
  .ensure((c: ILogin) => c.password).required().withMessage("* Please enter your password");

// export const saleHeader = ValidationRules
//   .ensure((c: ISaleHeader) => c.date).required().withMessage("* Please provide the data")
//   .ensure((c: ISaleHeader) => c.shipperId).required().withMessage("* Please provide the shipper")
//   .ensure((c: ISaleHeader) => c.buyerId).required().withMessage("* Please provide the buyer")
//   .ensure((c: ISaleHeader) => c.incotermId).required().withMessage("* Please provide the shipment basis")
//   .ensure((c: ISaleHeader) => c.paymentTermId).required().withMessage("* Please provide the payment terms")
//   .ensure((c: ISaleHeader) => c.portOfLoadingId).required().withMessage("* Please provide the port of loading")
//   .ensure((c: ISaleHeader) => c.productId).required().withMessage("* Please provide the product");

// export const userRegistration = ValidationRules
//   .ensure((c: IUserRegistrationRequest) => c.email).required().withMessage("* Please provide your email")
//   .ensure((c: IUserRegistrationRequest) => c.firstName).required().withMessage("* Please provide your first name")
//   .ensure((c: IUserRegistrationRequest) => c.lastName).required().withMessage("* Please provide your last name")
//   .ensure((c: IUserRegistrationRequest) => c.password).required().withMessage("* Please provide the password")
//   .ensure((c: IUserRegistrationRequest) => c.companyName).required().withMessage("Please provide the company name")
//   .ensure((c: IUserRegistrationRequest) => c.taxId).required().withMessage("* Please provide the Tax Id");

// export const saleDetail = ValidationRules
//   .ensure((c: ISaleDetail) => c.quantityKg).satisfies(val => val > 0).withMessage("* Quantity must be greater than 0")
//   .ensure((c: ISaleDetail) => c.price).satisfies(val => val > 0).withMessage("* Price must be greater than 0")
//   .ensure((c: ISaleDetail) => c.destinationId).required().withMessage("* The destination is required")
//   .ensure((c: ISaleDetail) => c.gradeId).required().withMessage("* The grade is required");

ValidationRules.customRule(
  "dateAfter", (value, obj, otherPropertyName) => moment(value).isBefore(obj[otherPropertyName]), "");

// export const shipmentPeriod = ValidationRules
//   .ensure((c: IShipmentPeriod) => c.from).required()
//   .satisfiesRule("dateAfter", "to").withMessage("* The start of the shipment period must be before the end of the shipping period");
