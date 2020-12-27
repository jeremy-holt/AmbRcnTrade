export interface IAuditLog {
  date: string;
  userRole: string;
  appUserId: string;
  userName: string;
  email: string;
  page: string;
  queryString: string;
  ipAddress: string;
  id: string;
}
