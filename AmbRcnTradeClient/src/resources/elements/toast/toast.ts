import { autoinject, bindable, observable } from "aurelia-framework";
import { connectTo } from "aurelia-store";
import iziToast from "izitoast";
import { IState } from "../../../store/state";
import { trueFalse } from "./../../../core/helpers";

@autoinject
@connectTo()
export class Toast {
  @observable public state: IState = undefined!;
  @bindable public delay = 2000;
  @bindable public showErrors = true;

  private isReady = false;
  private lastServerMessage = "";
  private lastErrorMessage = "";

  protected stateChanged(state: IState) {
    this.setNotification(state);
  }

  protected setNotification(state: IState) {
    if (!this.isReady) {
      return;
    }

    if (this.showErrors && state.serverMessages.errorMessage && state.serverMessages.errorMessage !== this.lastErrorMessage) {
      iziToast.error({ title: "Error", message: state.serverMessages.errorMessage });
      this.lastErrorMessage = state.serverMessages.errorMessage;
    }

    if (state.serverMessages.message && state.serverMessages.message !== this.lastServerMessage) {
      iziToast.success({ title: "", message: state.serverMessages.message });
      this.lastServerMessage = state.serverMessages.message;
    }
  }

  protected bind() {
    this.showErrors = trueFalse(this.showErrors) as boolean;
    this.setSettings();
    this.isReady = true;
  }

  // protected unbind() {
  //   iziToast.destroy();
  // }

  private setSettings() {
    iziToast.settings({
      transitionIn: "flipInX",
      transitionOut: "flipOutX",
      position: "topRight",
      closeOnClick: true,
      timeout: this.delay,
      maxWidth: 250
    });
  }
}
