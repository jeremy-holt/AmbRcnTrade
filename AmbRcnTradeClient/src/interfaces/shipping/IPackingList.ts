import { IContainer } from "./IContainer";

export interface IPackingList {
    id: string;
    name: string;
    companyId: string;
    containerIds: string[];
    bookingNumber: string;
    date: string;
    notes: string;
    containers: IContainer[];
    shipperId: string;
    freightForwarderId: string;
    warehouseId: string;
    otNo: number;
    dateStart:  string;
    dateEnd:  string;
    contractNumber: string;
    amqNo: string;
    numberContainers: number;
    vesselName: string;
    customerId: string;
    destinationId: string;
    destinationCountry: string;
    packingListNumber: string;
    representative: string;
}
