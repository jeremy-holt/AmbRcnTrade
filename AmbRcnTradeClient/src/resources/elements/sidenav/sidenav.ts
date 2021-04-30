import { EventAggregator, Subscription } from "aurelia-event-aggregator";
import { autoinject } from "aurelia-framework";
import { NavModel, Router } from "aurelia-router";
import { connectTo } from "aurelia-store";
import { isInRole } from "core/services/role-service";
import { IState } from "../../../store/state";
import { AuthenticationService } from "core/services/authentication-service";

@autoinject
@connectTo()
export class Sidenav {
  public state: IState = undefined!;
  public open = false;
  private subscriptions: Subscription[] = [];

  private wasOpenedFromButton = false;

  constructor(
    ea: EventAggregator,
    private authService: AuthenticationService,
    private router: Router
  ) {
    this.subscriptions.push(ea.subscribe("openSideNav", () => this.openSideNav()));
    this.subscriptions.push(ea.subscribe("closeSideNav", () => this.close()));
  }

  public close() {
    if (!this.wasOpenedFromButton) {
      this.open = false;
    }
  }

  protected openSideNav() {
    this.open = true;
    this.wasOpenedFromButton = true;

    setTimeout(() => {
      this.wasOpenedFromButton = false;
    }, 2000);

  }

  protected dictionaryRoutes() {
    if (isInRole(["admin", "user"], this.state)) {
      return this.router.navigation.filter(row => this.isInRoute(row, "dictionary"));
    }
  }

  protected adminRoutes() {
    if (isInRole(["admin"], this.state)) {
      return this.router.navigation.filter(row => this.isInRoute(row, "admin"));
    }
  }

  protected inspectionRoutes() {
    if (isInRole(["admin", "user", "warehouseManager", "inspector"], this.state)) {
      return this.router.navigation.filter(row => this.isInRoute(row, "inspections"));
    }
  }

  protected stockRoutes() {
    if (isInRole(["admin", "user", "warehouseManager"], this.state)) {
      return this.router.navigation.filter(row => this.isInRoute(row, "stock"));
    }
  }

  protected containerRoutes() {
    if (isInRole(["admin", "user", "warehouseManager"], this.state)) {
      return this.router.navigation.filter(row => this.isInRoute(row, "container"));
    }
  }

  protected vesselRoutes() {
    if (isInRole(["admin", "user"], this.state)) {
      return this.router.navigation.filter(row => this.isInRoute(row, "vessel"));
    }
  }

  protected packingListRoutes(){
    if(isInRole(["admin","user","warehouseManager"],this.state)){
      return this.router.navigation.filter(row=>this.isInRoute(row,"packingList"));
    }
  }

  protected paymentRoutes() {
    if (isInRole(["admin", "user"], this.state)) {
      return this.router.navigation.filter(row => this.isInRoute(row, "payment"));
    }
  }

  protected purchaseRoutes() {
    if (isInRole(["admin", "user"], this.state)) {
      return this.router.navigation.filter(row => this.isInRoute(row, "purchase"));
    }
  }

  protected get isAdmin() {
    return isInRole(["admin"], this.state);
  }

  // private isNotInRoute(row: NavModel, exclusions: string[]) {
  //   return !exclusions.some(ex => row.config.route.toString().startsWith(ex));
  // }


  private isInRoute(row: NavModel, route: string) {
    return row.config.route.toString().startsWith(route);
  }

  protected detached() {
    this.subscriptions.forEach(c => c.dispose());
  }

  protected async logout() {
    await this.authService.logout();
    this.open = false;
    this.router.navigateToRoute("home");
  }

  protected login() {
    this.open = false;
    this.router.navigateToRoute("login");
  }
}
