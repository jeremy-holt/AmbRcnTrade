export interface IMoveInspectionToStockRequest {
  inspectionId: string;
  bags: number;
  weightKg: number;
  date: string;
  locationId: string;
  lotNo: number;
  origin: string;
}
