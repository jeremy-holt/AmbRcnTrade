export interface IIncomingStock {    
    bags: number;
    weightKg: number;
    lotNo: number;
    stockIds: IIncomingStockItem[];
    stuffingDate: string;
    kor: number;
}

export interface IIncomingStockItem{
  stockId: string;
  isStockIn: boolean;
}

export interface IOutgoingStock {
    stockId: string;
}
