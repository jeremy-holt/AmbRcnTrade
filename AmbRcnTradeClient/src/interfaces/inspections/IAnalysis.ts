import { Approval } from "constants/app-constants";

export interface IAnalysis {
    moisture: number;
    count: number;
    spotted: number;
    sound: number;
    rejects: number;
    kor: number;
    approved: Approval;
    bags: number;
}
