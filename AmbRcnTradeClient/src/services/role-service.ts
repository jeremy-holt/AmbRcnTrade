import { isArray } from "util";
import { concatStringArray } from "../core/helpers";
import { IRoleNameItem } from "core/interfaces/IRoleNameItem";
import { IState } from "../store/state";
import { RoleType } from "core/interfaces/types";
export class RoleService {

}

export const isInRole = (allowedRoles: RoleType[] | RoleType | string, state: Partial<IState>) => {
  if (state === undefined || state?.user === undefined || state?.user?.role === undefined) {
    return false;
  }

  if (!state.loggedIn) {
    return false;
  }

  if (state.user.role.includes("admin")) {
    return true;
  }

  if (state.loggedIn && allowedRoles.length === 0) {
    return true;
  }

  if (isArray(allowedRoles)) {
    return allowedRoles.filter(c => state.user?.role.includes(c)).length > 0;
  }

  const splits = allowedRoles.split(",").map(c => c as RoleType);  /*?*/
  return splits.filter(c => state.user?.role.includes(c)).length > 0;
};

export const roleToString = (role: string, state: IState, admin: RoleType) => {
  const roles = role ? role.split(",") : [];
  const list: string[] = [];
  roles.forEach(r => {
    list.push(state[admin].roleNames.find((c: IRoleNameItem) => c.id === r)?.name);
  });

  return roles.length > 0 ? concatStringArray(list, ", ") : "";
};
