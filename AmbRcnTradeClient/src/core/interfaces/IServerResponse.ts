
export interface IServerResponse<T> {
  dto: T;
  message: string;
  stackTrace: string;
  isError: boolean;
}
