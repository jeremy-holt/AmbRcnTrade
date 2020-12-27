import { IAppUserPassword } from "./../../../interfaces/IAppUserPassword";
import { UserService } from "./../../../services/user-service";
import { autoinject } from "aurelia-framework";

@autoinject
export class AppUserPasswordsList {
  public list: IAppUserPassword[] = [];

  constructor(
    private userService: UserService
  ) { }

  protected async activate() {
    this.list = await this.userService.loadAppUsersPasswords();
  }

  protected async save() {
    await this.userService.saveAppUsersPasswords(this.list);
    await this.userService.loadAppUsersPasswords();
  }

  protected addNew() {
    this.list.push({} as IAppUserPassword);
  }

  protected delete(index: number) {
    this.list.splice(index, 1);
  }
}
