import { IAppUserPassword } from "core/interfaces/IAppUserPassword";
import { AppUserService } from "../../../services/app-user-service";
import { autoinject } from "aurelia-framework";

@autoinject
export class AppUserPasswordsList {
  public list: IAppUserPassword[] = [];

  constructor(
    private userService: AppUserService
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
