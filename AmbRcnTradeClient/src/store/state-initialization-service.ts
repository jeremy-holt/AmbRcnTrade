import { autoinject } from "aurelia-framework";
import { Store } from "aurelia-store";
import _ from "lodash";
import { initialState } from "./initial-state";
import { IState } from "./state";

@autoinject
export class StateInitializationService {
  constructor(
    private store: Store<IState>
  ) {
    store.registerAction("initializeState", initializeStateAction);
  }

  public init(state: IState) {
    Object.keys(initialState).forEach(key => {
      if (state && state[key] === undefined) {
        state[key] = initialState[key];
      }
      if (initialState[key]?.hasOwnProperty("current") && !state[key].current) {
        state[key].current = initialState[key].current;
      }
    });

    state.serverMessages.errorMessage = undefined!;
    state.serverMessages.message = undefined!;

    this.store.dispatch(initializeStateAction, state);
  }
}

export function initializeStateAction(state: IState, response: IState) {
  let newState = _.cloneDeep(state);
  newState = response;
  return newState;
}
