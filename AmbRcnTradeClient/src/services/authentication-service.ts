import { AuthService } from "aurelia-authentication";
import { autoinject, TaskQueue } from "aurelia-framework";
import { I18N } from "aurelia-i18n";
import { Router } from "aurelia-router";
import { Store } from "aurelia-store";
import _ from "lodash";
import { log } from "../core/log";
import { IPayload } from "../interfaces/IPayload";
import { initialState } from "../store/initial-state";
import { IState } from "../store/state";
import { LOCAL_STORAGE } from "./../localStorage-consts";
import { AdminService } from "./admin-service";
import { ServerMessageService } from "./server-message-service";
import { AppUserService } from "./app-user-service";

@autoinject
export class AuthenticationService {
  constructor(
    private auth: AuthService,
    public store: Store<IState>,
    private serverMessageService: ServerMessageService,
    private adminService: AdminService,
    private userService: AppUserService,
    private taskQueue: TaskQueue,
    private i18n: I18N,
    private router: Router) {

    store.registerAction("setUserAction", setUserAction);
    store.registerAction("setCurrentCompanyAction", setCurrentCompanyAction);
    store.registerAction("logoutAction", logoutAction);
    // store.registerAction("setForceSelectCompanyAction", setForceSelectCompanyAction);
  }

  public get authToken() {
    return this.auth.getAccessToken();
  }

  public async login(email: string, password: string) {
    await this.serverMessageService.clearMessages();

    let isMemberOfCompany = false;

    try {
      await this.auth.login(email, password);

      const payload = this.auth.getTokenPayload() as any;

      const user: IPayload = {
        email: payload.email,
        id: payload.primarysid,
        firstName: payload.given_name,
        lastName: payload.family_name,
        name: `${payload.given_name} ${payload.family_name}`,
        role: payload.role,
        companies: payload.companies.split(",")
      };

      switch (user.companies.length) {
        case 0:
          isMemberOfCompany = false;
          await this.setCurrentCompany("");
          break;
        case 1:
          await this.setCurrentCompany(user.companies[0]);
          isMemberOfCompany = true;
          break;
        default:
          isMemberOfCompany = true;
          await this.adminService.loadCompaniesList();
          // await this.store.dispatch(setForceSelectCompanyAction, true);
          await this.setCurrentCompany(user.companies[0]);
          break;
      }

      if (!isMemberOfCompany) {
        await this.serverMessageService.setErrorMessage("You are logged in to the system, but you have not been allocated to a company. Please contact the System Administator");
      }

      const isLoggedIn = this.auth.isAuthenticated() && isMemberOfCompany;

      await this.userService.getCompaniesForUser(user);

      await this.store.dispatch(setUserAction, user, isLoggedIn);

      return true;
    } catch (err) {
      const text = await err.text();
      try {
        const json = JSON.parse(text);
        this.taskQueue.queueMicroTask(async () => {
          await this.serverMessageService.setErrorMessage(this.i18n.tr(json.message));
        });
      } catch (e) {
        log.error("Login error", e);
      } finally {
        this.store.dispatch(logoutAction);
      }
    }
  }

  public async setCurrentCompany(companyId: string) {
    if (companyId) {
      await this.adminService.getCompanyName(companyId);
    }

    return this.store.dispatch(setCurrentCompanyAction, companyId);
  }

  public async logout() {
    await this.store.dispatch(logoutAction);
    this.taskQueue.queueMicroTask(() => {
      this.router.navigate("home");
    });
  }
}

// ACTIONS
export async function setUserAction(state: IState, user: IPayload, isLoggedIn: boolean) {
  const newState = _.cloneDeep(state);
  newState.user = user;
  newState.loggedIn = isLoggedIn;
  return newState;
}

export async function setCurrentCompanyAction(state: IState, companyId: string) {
  const newState = _.cloneDeep(state);
  newState.currentCompanyId = companyId;
  return newState;
}

export async function logoutAction(state: IState) {
  const newState = initialState;
  localStorage.removeItem(LOCAL_STORAGE.aurelia_authentication);
  return newState;
}

// export async function setForceSelectCompanyAction(state: IState, forceSelectCompany: boolean) {
//   const newState = _.cloneDeep(state);
//   newState.forceSelectCompany = forceSelectCompany;
//   return newState;
// }
