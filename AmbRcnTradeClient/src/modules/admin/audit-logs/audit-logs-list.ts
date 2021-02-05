import { autoinject } from "aurelia-framework";
import { IAuditLog } from "./../../../interfaces/IAuditLog";
import { AuditingService } from "./../../../services/auditing-service";

@autoinject
export class AuditLogsList {
  public list: IAuditLog[] = [];
  public daysToClear = 30;
  protected uniqueUsers: string[] = [];

  constructor(
    private auditService: AuditingService
  ) { }

  protected async bind() {
    await this.loadList();
    this.getUniqueUsers();
  }

  protected async clearLogs() {
    await this.auditService.clearLogs(this.daysToClear);
    await this.loadList();
  }


  private async loadList() {
    this.list = await this.auditService.loadList();
  }

  private getUniqueUsers() {
    this.uniqueUsers = Array.from(new Set(this.list.map(item => `${item.userName} (${item.userRole})`)));
    this.uniqueUsers.sort();
    this.uniqueUsers = this.uniqueUsers.filter(c => c);
  }
}
