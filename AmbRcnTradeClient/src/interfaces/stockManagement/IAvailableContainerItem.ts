import { ContainerStatus } from "constants/app-constants";
export interface IAvailableContainer {
    status: ContainerStatus;
    containerNumber: string;
    bookingNumber: string;
    bags: number;
    stockWeightKg: number;
    id: string;
    isOverweight: boolean;
    warehouseId: string;
    selected?:boolean;
}
