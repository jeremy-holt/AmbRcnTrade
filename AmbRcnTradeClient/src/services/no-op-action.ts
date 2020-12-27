import _ from "lodash";
import { IState } from "../store/state";

export function noOpAction(state: IState) {
  const newState = _.cloneDeep(state);
  return newState;
}
