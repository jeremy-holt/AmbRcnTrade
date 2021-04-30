import { RoleType } from "./types";

export interface IPayload {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  name: string;
  role: RoleType[];
  companies: string[];
}
