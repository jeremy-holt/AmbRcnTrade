import { Subscription } from "aurelia-event-aggregator";
import { autoinject, observable } from "aurelia-framework";
import { connectTo } from "aurelia-store";
import { ValidateEvent, validateTrigger, ValidationController, ValidationControllerFactory } from "aurelia-validation";
import { AuthenticationService } from "core/services/authentication-service";
import { IState } from "../../store/state";
import * as validationRules from "../validation-rules";
import { ILogin } from "core/interfaces/ILogin";
import { BootstrapFormRenderer } from "./../../resources/custom-renderers/bootstrap-form-renderer";
import { ServerMessageService } from "core/services/server-message-service";

@autoinject
@connectTo()
export class LoginEdit {
  @observable public state: IState = undefined!;
  public model: ILogin = undefined!;

  public canLogin = false;
  private controller: ValidationController;
  private subscriptions: Subscription[] = [];
  protected statusCodeMessage = "";

  constructor(
    private authService: AuthenticationService,
    private serverMessageService: ServerMessageService,
    factory: ValidationControllerFactory
  ) {
    this.controller = factory.createForCurrentScope();
    this.controller.validateTrigger = validateTrigger.changeOrBlur;
    this.controller.addRenderer(new BootstrapFormRenderer());
    this.subscriptions.push(this.controller.subscribe(ev => this.validate(ev)));
  }

  protected validate(ev: ValidateEvent) {
    this.canLogin = ev.errors.length === 0;
  }

  protected bind() {
    this.model = { email: "", password: "" };
    validationRules.login.on(this.model);
  }

  protected async login() {
    await this.controller.validate();

    if (this.canLogin) {
      await this.authService.login(this.model.email, this.model.password);
    }
  }

  protected async activate(params: { httpStatusCode: string }) {
    if (params) {
      switch (params.httpStatusCode) {
        case "401":
          this.statusCodeMessage = "You need to login before accessing the sytem";
          break;
        case "403":
 
          this.statusCodeMessage = "You are not authorized to view this page. Please check with the system administrator";
          break;
        default:          
          break;
      }
    }

    this.serverMessageService.clearMessages();
  }


  protected detached() {
    this.subscriptions.forEach(c => c.dispose());
  }
}
