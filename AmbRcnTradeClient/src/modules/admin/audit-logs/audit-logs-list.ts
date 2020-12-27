import { autoinject } from "aurelia-framework";
import { IAuditLog } from "./../../../interfaces/IAuditLog";
import { AuditingService } from "./../../../services/auditing-service";

@autoinject
export class AuditLogsList {
  public list: IAuditLog[] = [];
  public daysToClear = 30;

  constructor(
    private auditService: AuditingService
  ) { }

  protected async bind() {
    await this.loadList();
  }

  protected async clearLogs() {
    await this.auditService.clearLogs(this.daysToClear);
    await this.loadList();
  }
 

  private async loadList() {
    this.list = await this.auditService.loadList();
  }
}
