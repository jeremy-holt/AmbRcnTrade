export interface IIncomingStock {    
    bags: number;
    weightKg: number;
    lotNo: number;
    stockIds: string[];
    stuffingDate: string;
}

export interface IOutgoingStock {
    stockId: string;
}
