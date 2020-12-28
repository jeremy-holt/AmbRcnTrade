import moment from "moment";
import { Sidenav } from "./resources/elements/sidenav/sidenav";
import { autoinject, observable } from "aurelia-framework";
import { connectTo, localStorageMiddleware, MiddlewarePlacement, rehydrateFromLocalStorage, Store } from "aurelia-store";
import { IState } from "./store/state";
import { StateInitializationService } from "./store/state-initialization-service";
import { LOCAL_STORAGE } from "./localStorage-consts";
import { Router, RouterConfiguration } from "aurelia-router";
import { AuthenticationService } from "./services/authentication-service";
import { HttpClient } from "aurelia-fetch-client";
import { FetchConfig } from "aurelia-authentication";
import { EventAggregator } from "aurelia-event-aggregator";
import { noOpAction } from "./services/no-op-action";

@autoinject
@connectTo()
export class App {
  @observable public state: IState = undefined!;
  public router: Router = undefined!;
  public sideName: Sidenav = undefined!;

  constructor(
    public auth: AuthenticationService,
    public http: HttpClient,
    private fetchConfig: FetchConfig,
    private ea: EventAggregator,
    private stateInitializationService: StateInitializationService,
    store: Store<IState>
  ) {
    const authToken = this.auth.authToken;
    moment.locale("en");

    http.configure(config => {
      config.withDefaults({
        headers: {
          Accept: "application/json",
          Authorization: authToken
        }
      });
    });

    store.registerAction("Rehydrate", rehydrateFromLocalStorage);
    store.registerAction("noOpAction", noOpAction);

    if (localStorage[LOCAL_STORAGE.state]) {
      store.dispatch(rehydrateFromLocalStorage, LOCAL_STORAGE.state);
    }
    store.registerMiddleware(localStorageMiddleware, MiddlewarePlacement.After, { key: LOCAL_STORAGE.state });
  }

  public configureRouter(config: RouterConfiguration, router: Router) {
    this.router = router;
    config.title = "RCN Trade";
    config.map(
      [
        { route: ["", "home"], name: "home", title: "Home", nav: true, moduleId: "modules/home/home-edit" },
        { route: "login", name: "login", title: "Login", moduleId: "modules/login/login-edit" },


        // Inspections
        { route: "inspections/list/:approval?", href: "inspections/list", name: "inspectionList", title: "Inspections", nav: true, moduleId: "modules/inspection/inspection-list", roles: ["admin", "user", "guest", "inspector"] },
        { route: "inspections/edit/:id?", name: "inspectionEdit", title: "Inspection", moduleId: "modules/inspection/inspection-edit", roles: ["admin", "user", "inspector"] },
        { route: "inspections/upload", name: "inspectionImageUpload", moduleId: "modules/inspection/file-manager/file-manager" },
        { route: "inspections/print/:id", name: "inspectionPrint", title: "Print", moduleId: "modules/inspection/inspection-print", roles: ["admin", "user", "guest", "inspector"] },

        // Stocks
        // {route: "stock/list/:lotNo?/:locationId?", name="stockList",title="Stocks",nav:true,moduleId:"modules/stock/stock-list",roles:["admin", "user","guest", "inspector"]},

        // Dictionary items       
        { route: "dictionary/customer/edit", name: "customerEdit", title: "Customers", nav: true, moduleId: "modules/dictionary/customer/customer-edit", roles: ["admin"] },
        { route: "dictionary/port/edit", name: "portEdit", title: "Ports", nav: true, moduleId: "modules/dictionary/port/port-edit", roles: ["admin"] },

        // Admin
        { route: "admin/appUser/edit", name: "appUserEdit", title: "Users", nav: true, moduleId: "modules/admin/app-user/app-user-edit", roles: ["admin"] },
        { route: "admin/customerUsers/list", name: "customerUserList", title: "Customers/Users", nav: true, moduleId: "modules/admin/customer-users/customer-user-list", roles: ["admin"] },
        { route: "admin/userPasswords/list", name: "userPasswordsList", title: "User passwords", nav: true, moduleId: "modules/admin/app-user-passwords/app-user-passwords-list", roles: ["admin"] },
        { route: "admin/logs/list", name: "auditLogs", title: "Audit logs", nav: true, moduleId: "modules/admin/audit-logs/audit-logs-list", roles: ["admin"] }
      ]
    );
  }

  public activate() {
    this.fetchConfig.configure(this.http);
  }

  protected closeSideNav() {
    this.ea.publish("closeSideNav");
  }

  protected bind() {
    this.stateInitializationService.init(this.state);
  }
}
