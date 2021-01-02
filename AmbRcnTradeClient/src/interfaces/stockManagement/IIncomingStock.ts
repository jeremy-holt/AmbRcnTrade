export interface IIncomingStock {
    stockId: string;
    bags: number;
    weightKg: number;
    lotNo: number;
    stockIds: string[];
    stuffingDate: string;
}

export interface IOutgoingStock {
    stockId: string;
}
