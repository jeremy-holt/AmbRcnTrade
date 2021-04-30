import { autoinject } from "aurelia-dependency-injection";
import { Router } from "aurelia-router";
import { connectTo } from "aurelia-store";
import { IState } from "store/state";
import { AuthenticationService } from "core/services/authentication-service";
import { isInRole } from "core/services/role-service";
@autoinject
@connectTo()
export class HomeEdit {
  public state: IState = undefined!;

  constructor(
    private authService: AuthenticationService,
    private router: Router
  ) { }

  protected async logout() {
    await this.authService.logout();    
    this.router.navigateToRoute("home");
  } 
  
  protected get isGuest(){
    return this.state && this.state.loggedIn && isInRole(["guest"],this.state);
  }

  protected get isInspector(){
    return this.state && this.state.loggedIn && isInRole(["inspector"],this.state);
  }

  protected get isUser(){
    return this.state && this.state.loggedIn && isInRole(["user"], this.state);
  }

  protected get isAdmin(){
    return this.state && this.state.loggedIn && isInRole("admin",this.state);
  }

  protected get requireLogin(){
    return !this.state?.loggedIn || !this.state?.user?.id || !this.authService.authToken;
  }
}
