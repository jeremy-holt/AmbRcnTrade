import { ContainerStatus } from "constants/app-constants";
export interface IAvailableContainerItem {
    status: ContainerStatus;
    containerNumber: string;
    bookingNumber: string;
    bags: number;
    stockWeightKg: number;
    containerId: string;
    isOverweight: boolean;

    selected?:boolean;
}
