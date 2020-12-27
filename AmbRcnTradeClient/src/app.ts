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
    config.title = "Brokerage";
    config.map(
      [
        { route: ["", "home"], name: "home", title: "Home", nav: true, moduleId: "modules/home/home-edit" },
        { route: "login", name: "login", title: "Login", moduleId: "modules/login/login-edit" },

        // Contract
        { route: "contract/edit/:id?/:containerId?/:form?", name: "contractEdit", title: "Contract", moduleId: "modules/contract/contract-edit", roles: ["admin", "user"] },
        { route: "contract/list/:sellerId?/:buyerId?/:containerStatus?", href: "contract/list", name: "contractList", title: "Contracts", nav: true, moduleId: "modules/contract/contract-list", roles: ["admin", "user", "guest", "inspector"] },
        { route: "pre-shipment-data/list", name: "preShipmentDataList", title: "Pre shipment data", nav: true, moduleId: "modules/contract/pre-shipment-list", roles: ["admin", "user", "guest", "inspector"] },
        { route: "contract/print/:id", name: "contractPrint", title: "Contract - print", moduleId: "modules/contract/contract-print", roles: ["admin"] },
        { route: "contract-document-viewer/:id", name:"contractDocumentViewer", title: "Document viewer", moduleId: "modules/contract/document-viewer", roles: ["admin", "user", "guest"] },

        // Accounts
        { route: "accounts/chartOfAccounts/edit", name: "chartOfAccountsEdit", title: "Chart of Accounts", nav: true, moduleId: "modules/chart-of-account/chart-of-account-edit", roles: ["admin"] },
        { route: "accounts/transactions/create", name: "transactionCreate", title: "Create transactions", nav: true, moduleId: "modules/transaction/transaction-create", roles: ["admin"] },
        { route: "accounts/transaction/edit/:id?", name: "transactionEdit", title: "Edit transaction", moduleId: "modules/transaction/transaction-edit", roles: ["admin"] },
        { route: "accounts/statement/edit/:chartOfAccountId/:accountCodeId", name: "statementEdit", title: "Statement", moduleId: "modules/statement/statement-edit", roles: ["admin"] },
        { route: "accounts/statement/bank/:chartOfAccountId/:accountCodeId", name: "statementBankEdit", title: "Statement", moduleId: "modules/statementBank/statement-bank-edit", roles: ["admin"] },
        { route: "accounts/statement/expense/:chartOfAccountId/:accountCodeId", name: "statementExpenseEdit", title: "Statement", moduleId: "modules/statementExpense/statement-expense-edit", roles: ["admin"] },

        // Misc
        { route: "inspections/list/:inspectionId?/:completed?", href: "inspections/list", name: "inspectionList", title: "Inspections", nav: true, moduleId: "modules/inspections/inspection-list", roles: ["admin", "user", "guest", "inspector"] },
        { route: "inspections/edit/:id?", name: "inspectionEdit", title: "Inspection", moduleId: "modules/inspections/inspection-edit", roles: ["admin", "user", "inspector"] },
        { route: "inspections/spec-by-grade/list/:gradeId?/:specificationId?", href: "inspections/spec-by-grade/list", name: "tolerancesList", title: "Quality specifications", nav: true, moduleId: "modules/inspections/spec-by-grade/spec-by-grade-list", roles: ["admin"] },
        { route: "inspections/upload", name: "inspectionImageUpload", moduleId: "modules/inspections/file-manager/file-manager" },
        { route: "inspections/print/:id", name: "inspectionPrint", title: "Print", moduleId: "modules/inspections/inspection-print", roles: ["admin", "user", "guest", "inspector"] },

        // Dictionary items       
        { route: "dictionary/customer/edit", name: "customerEdit", title: "Customers", nav: true, moduleId: "modules/dictionary/customer/customer-edit", roles: ["admin"] },
        { route: "dictionary/commodityGroup/edit", name: "commodityGroupEdit", title: "Commodity groups", nav: true, moduleId: "modules/dictionary/commodityGroup/commodityGroup-edit", roles: ["admin"] },
        { route: "dictionary/commodity/edit", name: "commodityEdit", title: "Commodities", nav: true, moduleId: "modules/dictionary/commodity/commodity-edit", roles: ["admin"] },

        { route: "dictionary/grade/edit/:id?", name: "gradeEdit", title: "Grades", moduleId: "modules/dictionary/grade/grade-edit", roles: ["admin"] },
        { route: "dictionary/grade/list/:commodityGroupId?", href: "dictionary/grade/list", name: "gradeList", title: "Grades", nav: true, moduleId: "modules/dictionary/grade/grade-list", roles: ["admin"] },

        { route: "dictionary/payment/edit", name: "paymentEdit", title: "Payments", nav: true, moduleId: "modules/dictionary/payment/payment-edit", roles: ["admin"] },
        { route: "dictionary/packing/edit", name: "packingEdit", title: "Packing", nav: true, moduleId: "modules/dictionary/packing/packing-edit", roles: ["admin"] },
        { route: "dictionary/incoterm/edit", name: "incotermEdit", title: "Incoterms", nav: true, moduleId: "modules/dictionary/incoterm/incoterm-edit", roles: ["admin"] },
        { route: "dictionary/port/edit", name: "portEdit", title: "Ports", nav: true, moduleId: "modules/dictionary/port/port-edit", roles: ["admin"] },
        { route: "dictionary/transportUnit/edit", name: "transportUnitEdit", title: "Transport units", nav: true, moduleId: "modules/dictionary/transportUnit/transport-unit-edit", roles: ["admin"] },
        { route: "dictionary/clauseGroup/edit", name: "clauseGroupEdit", title: "Contract clause groups", nav: true, moduleId: "modules/dictionary/clauseGroup/clause-group-edit", roles: ["admin"] },
        { route: "dictionary/contractClause/list", name: "contractClauseList", title: "Contract clauses", nav: true, moduleId: "modules/dictionary/contract-clause/contract-clause-list", roles: ["admin"] },
        { route: "dictionary/contractClause/edit/:id?", name: "contractClauseEdit", title: "Contract clause", moduleId: "modules/dictionary/contract-clause/contract-clause-edit", roles: ["admin"] },
        { route: "dictionary/standardClause/edit", name: "standardClauseEdit", title: "Standard clauses", nav: true, moduleId: "modules/dictionary/standardClause/standard-clause-edit", roles: ["admin"] },
        { route: "dictionary/specifications/list", name: "specificationsList", title: "Specifications", nav: true, moduleId: "modules/inspections/specifications/specifications-list", roles: ["admin", "user"] },

        // Admin
        { route: "admin/commissions", name: "commissions", title: "Commissions", moduleId: "modules/commissions/commission-report-list", nav: true, roles: ["admin"] },
        { route: "admin/appUser/edit", name: "appUserEdit", title: "Users", nav: true, moduleId: "modules/admin/app-user/app-user-edit", roles: ["admin"] },
        { route: "admin/customerUsers/list", name: "customerUserList", title: "Customers/Users", nav: true, moduleId: "modules/admin/customer-users/customer-user-list", roles: ["admin"] },
        { route: "admin/toleranceDescriptor/list", name: "toleranceDescriptorList", title: "Tolerance descriptions", nav: true, moduleId: "modules/inspections/tolerance-descriptor/tolerance-descriptor-list", roles: ["admin"] },
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
