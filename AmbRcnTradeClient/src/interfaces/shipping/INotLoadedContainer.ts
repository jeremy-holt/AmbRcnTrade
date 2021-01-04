import { ContainerStatus } from "constants/app-constants";

export interface INotLoadedContainer {
    containerId: string;
    vesselId: string;
    containerNumber: string;
    bookingNumber: string;
    status: ContainerStatus;
    bags: number;
    companyId: string;
}
