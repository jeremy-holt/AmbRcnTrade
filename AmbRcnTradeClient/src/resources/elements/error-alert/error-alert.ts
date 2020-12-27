import { autoinject, bindable, observable } from "aurelia-framework";
import { connectTo, Store } from "aurelia-store";
import { serverClearMessageAction } from "../../../services/server-message-service";
import { IState } from "../../../store/state";

@autoinject
@connectTo()
export class ErrorAlert {
  @observable public state: IState = undefined!;
  @bindable public class: string = undefined!;

  public errorMessage: string = undefined!;

  constructor(
    private store: Store<IState>
  ) { }

  protected stateChanged(state: IState) {
    this.errorMessage = state.serverMessages.errorMessage;
  }
}
