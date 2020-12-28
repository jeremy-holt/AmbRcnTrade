import { Approval } from "./../../constants/app-constants";
export interface IInspectionQueryParams {
    companyId: string;
    approved: Approval | null;
}
