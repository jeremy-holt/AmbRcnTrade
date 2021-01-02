export interface IIncomingStock {
    stockId: string;
    bags: number;

    weightKg: number;
}

export interface IStuffingRequest {
    containerId: string;
    stuffingDate: string;
    incomingStocks: IIncomingStock[];
}

export interface IOutgoingStock {
    stockId: string;
}
