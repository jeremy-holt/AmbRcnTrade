import { Approval } from "constants/app-constants";

export interface IAnalysis {
    moisture: number;
    count: number;
    // spottedPct: number;
    spottedGm: number;
    // soundPct: number;
    soundGm: number;
    // rejectsPct: number;
    rejectsGm: number;
    kor: number;
}

export interface IAnalysisResult {
    moisture: number;
    count: number;
    spottedPct: number;
    soundPct: number;
    rejectsPct: number;
    kor: number;
    approved: Approval;
}
