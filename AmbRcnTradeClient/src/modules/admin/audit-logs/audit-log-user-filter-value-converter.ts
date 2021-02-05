import { IAuditLog } from "../../../interfaces/IAuditLog";

export class AuditLogUserFilterValueConverter {
  public toView(list: IAuditLog[], filter: string) {
    if (!filter || filter === "[All]") {
      return list;
    }

    return list.filter(c=>`${c.userName} (${c.userRole})` === filter);
  }
}
