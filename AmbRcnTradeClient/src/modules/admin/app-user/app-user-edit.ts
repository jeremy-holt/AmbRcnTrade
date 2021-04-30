import { ICompanyListItem } from "core/interfaces/ICompany";
import { autoinject, observable } from "aurelia-framework";
import { connectTo } from "aurelia-store";
import { IState } from "store/state";
import { IAppUser, IAppUserListItem } from "../../../core/interfaces/IAppUser";
import { IRoleNameItem } from "core/interfaces/IRoleNameItem";
import { RoleType } from "core/interfaces/types";
import { AdminService } from "../../../services/admin-service";
import { AppUserService } from "../../../services/app-user-service";
import _ from "lodash";

@autoinject
@connectTo()
export class AppUserEdit {
  @observable public state: IState = undefined!;
  public model: IAppUser=undefined!;
  public appUsersList: IAppUserListItem[] = [];
  public companiesList: ICompanyListItem[] = [];
  public roleNamesList: IRoleNameItem[] = [];
  public selectedRoles: string[] = [];
  public rolesMessage = "";

  @observable public selectedCompany: ICompanyListItem = undefined!;
  @observable public selectedAppUser: IAppUserListItem = undefined!;

  constructor(
    private userService: AppUserService,
    private adminService: AdminService
  ) { }

  protected async activate() {
    await this.adminService.loadUsersList();
    await this.adminService.getRoleNames();
    await this.adminService.loadCompaniesList();
  }

  protected stateChanged(state: IState) {
    this.appUsersList = state.admin.user.list;
    this.roleNamesList = state.admin.roleNames;
    this.companiesList = state.admin.company.list;
    this.model = state.admin.user.current;
    this.selectedCompany = this.selectedCompany ?? this.companiesList[0];

    this.setSelectedRoles(this.model);
  }

  protected async selectedAppUserChanged(item: IAppUserListItem) {
    if (item) {
      await this.adminService.loadUser(item.id);
    }
  }

  protected selectedRolesChanged(roles: string[]) {
    if (this.model) {
      this.model.role = roles.join(",") as RoleType;
      this.setRolesMessage();
    }
  }

  protected addUser() {
    this.model = {} as IAppUser;
    this.selectedRoles = [];
    this.setRolesMessage();
  }

  protected async save() {
    this.model.approved = true;
    // if (this.model.id) {
    //   await this.adminService.saveUser(this.model);
    // } else {
    //   await this.adminService.createUser(this.model);
    // }
    await this.adminService.saveUser(this.model);

    if (this.selectedCompany) {
      await this.adminService.loadCompany(this.selectedCompany.id);
      const currentCompany = _.cloneDeep(this.state.admin.company.current);
      if (!currentCompany.users.includes(this.model.id)) {
        currentCompany.users.push(this.model.id);
        await this.adminService.saveCompany(currentCompany);
      }
    }
    await this.adminService.loadUsersList();
  }

  protected get canSave() {
    return this.model?.email && this.model?.firstName && this.model?.lastName && this.model?.role;
  }

  protected get showPassword() {
    return this.model?.id === undefined;
  }

  private setSelectedRoles(user: IAppUser) {
    if (Array.isArray(user.role)) {
      this.selectedRoles = user.role;
    } else {
      this.selectedRoles = user?.role?.split(",") || [];
    }
    this.setRolesMessage();
  }

  private setRolesMessage() {
    this.rolesMessage = this.roleNamesList.filter(c => this.selectedRoles.includes(c.id)).map(c => c.name).join(", ") || "No roles";
  }
}
