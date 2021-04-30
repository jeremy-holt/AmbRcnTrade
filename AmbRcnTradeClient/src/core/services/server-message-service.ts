import { autoinject } from "aurelia-framework";
import { Store } from "aurelia-store";
import _ from "lodash";
import { log } from "core/log";
import { IState } from "store/state";

@autoinject
export class ServerMessageService {
  constructor(
    private store: Store<IState>
  ) {
    store.registerAction("serverClearMessageAction", serverClearMessageAction);
    store.registerAction("serverErrorMessageAction", serverErrorMessageAction);
    store.registerAction("serverMessageAction", serverMessageAction);
  }

  public async clearMessages() {
    return this.store.dispatch(serverClearMessageAction);
  }

  public async setMessage(message: string) {
    return this.store.dispatch(serverMessageAction, message);
  }

  public async setErrorMessage(message: string) {
    log.error(message);
    return this.store.dispatch(serverErrorMessageAction, message);
  }
}

export function serverClearMessageAction(state: IState) {
  const newState = _.cloneDeep(state);
  newState.serverMessages.message = "";
  newState.serverMessages.errorMessage = "";
  return newState;
}

export function serverMessageAction(state: IState, message: string) {
  const newState = _.cloneDeep(state);
  newState.serverMessages.errorMessage = "";
  newState.serverMessages.message = message;
  return newState;
}

export function serverErrorMessageAction(state: IState, message: string) {
  const newState = _.cloneDeep(state);
  newState.serverMessages.message = "";
  newState.serverMessages.errorMessage = message;
  return newState;
}
