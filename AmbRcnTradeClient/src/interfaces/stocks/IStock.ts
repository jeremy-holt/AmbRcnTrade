import { IEntityCompany } from "interfaces/IEntity";
import { IInspection } from "interfaces/inspections/IInspection";

export interface IStock extends IEntityCompany {
  id: string;
  name: string;
  companyId: string;
  locationId: string;
  locationName?: string;
  stockInDate: string | null;
  lotNo: number;
  bags: number;
  weightKg: number;
  inspectionId: string;
  inspection?: IInspection;
  stockOutDate: string | null;
  isStockIn: boolean;
  origin: string;
  supplierId: string;
  supplierName?: string;

  selected?: boolean;
}
