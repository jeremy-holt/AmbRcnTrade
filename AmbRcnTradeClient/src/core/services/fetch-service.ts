import { IEntity } from "core/interfaces/IEntity";
import { HttpClient, json } from "aurelia-fetch-client";
import { autoinject } from "aurelia-framework";
import { Router } from "aurelia-router";
import { Reducer, Store } from "aurelia-store";
import { IEntityCompany } from "core/interfaces/IEntity";
import { Subscription } from "rxjs";
import { log } from "core/log";
import { IServerResponse } from "core/interfaces/IServerResponse";
import { FetchRoute } from "./FetchRoute";
import { QueryId } from "./QueryId";
import { logoutAction } from "./authentication-service";
import { GetUrlService } from "./get-url-service";
import { serverErrorMessageAction, ServerMessageService } from "./server-message-service";
import { LOCAL_STORAGE } from "core/localStorage-consts";
import { IState } from "store/state";

export type RequestMethod = "get" | "delete" | "post" | "getMany";

@autoinject
export class FetchService {
  private getUrlService: GetUrlService;
  private serverMessageService: ServerMessageService;
  private authToken = "";

  constructor(
    private readonly baseUrl: string,
    protected readonly http: HttpClient,
    public store: Store<IState>,
    private router: Router
  ) {
    this.serverMessageService = new ServerMessageService(store);

    const accessToken = localStorage.getItem(LOCAL_STORAGE.aurelia_authentication);
    if (accessToken) {
      this.authToken = `Bearer ${JSON.parse(accessToken).access_token}`;
    }

    if (!baseUrl.startsWith("api/")) {
      throw new Error("The baseUrl should start with api/");
    }

    this.getUrlService = new GetUrlService(baseUrl);
  }

  public currentCompanyId() {
    return this.getCurrentState().currentCompanyId;
  }

  public currentCompanyIdQuery(): QueryId {
    return new QueryId("companyId", this.currentCompanyId());
  }

  public getCurrentState(): IState {
    let localState: IState = undefined!;
    const subscription: Subscription = this.store.state.subscribe(state => localState = state);
    subscription.unsubscribe();
    return localState;
  }

  public get isRequesting() {
    return this.http.isRequesting;
  }

  protected async create<T extends IEntityCompany | IEntity>(reducer: Reducer<IState, unknown[]>): Promise<void> {
    const url = `${this.baseUrl}/create`;

    await this.serverMessageService.clearMessages();

    try {
      const response = await this.http.fetch(url);
      try {
        const data = (await response.json()) as IServerResponse<T>;
        if (!data.isError) {
          (data.dto as IEntityCompany).companyId = this.currentCompanyId();

          await this.store.dispatch(reducer, data.dto);
        } else {
          await this.serverMessageService.setErrorMessage(data.message);
          await this.store.dispatch(reducer, undefined);
        }
      } catch (ex) {
        log.error(ex);
      }
    } catch (ex) {
      await this.handleErrorResponse(ex as Response);
    }
  }

  protected async get<T>(id: string | null | QueryId[], action: string, reducer?: Reducer<IState, unknown[]>): Promise<void | T> {
    const params = typeof (id) === "string" ? [new QueryId("id", id)] : id;
    const url = this.getUrlService.getUrl(new FetchRoute(params, action));

    try {
      const response = await this.http.fetch(url);
      return this.handleResponse<T>(response, reducer);
    } catch (ex) {
      return this.handleErrorResponse<T>(ex as Response);
    }
  }

  protected url(id: string | null | QueryId[], action: string) {
    const params = typeof (id) === "string" ? [new QueryId("id", id)] : id;
    return this.getUrlService.getUrl(new FetchRoute(params, action));
  }

  protected async getData<T>(id: QueryId[], action: string) {
    const url = this.getUrlService.getUrl(new FetchRoute(id, action));
    try {
      const response = await this.http.fetch(url);
      if (response.ok) {
        const data = await response.json();
        return data as T;
      }
    } catch (ex) {
      return this.handleErrorResponse<T>(ex as Response);
    }
  }

  protected async delete<T>(id: string | QueryId[], action: string, reducer: Reducer<IState, unknown[]>) {
    const params = typeof (id) === "string" ? [new QueryId("id", id)] : id;
    const url = this.getUrlService.getUrl(new FetchRoute(params, action));
    try {
      const response = await this.http.fetch(url, { method: "DELETE" });
      return this.handleResponse<T>(response, reducer);
    } catch (ex) {
      return this.handleErrorResponse<T>(ex as Response);
    }
  }

  protected async post<T>(model: unknown, action: string, reducer?: Reducer<IState, unknown[]>) {
    const url = this.getUrlService.getPostUrl(action);

    model["companyId"] = this.currentCompanyId();

    try {
      const response = await this.http.fetch(url, { method: "POST", body: json(model) });
      return this.handleResponse<T>(response, reducer);
    } catch (ex) {
      return this.handleErrorResponse<T>(ex as Response);
    }
  }

  protected async postImage(formData: FormData, headers: Headers, action: string) {
    const url = this.getUrlService.getPostUrl(action);

    const httpHeaders = new Headers();
    httpHeaders.append("Accept", "application/json");
    httpHeaders.append("Authorization", this.authToken);

    headers.forEach((value, name) => {
      httpHeaders.append(name, value);
    });

    try {
      const response = await this.http.fetch(
        url, {
          method: "post",
          body: formData,
          headers: httpHeaders
        }
      );
      return this.handleResponse(response, null);
    } catch (ex) {
      return this.handleErrorResponse(ex as Response);
    }
  }

  protected async getMany<TListItem>(queryParams: QueryId[], action: string, reducer?: Reducer<IState, unknown[]>) {
    const url = this.getUrlService.getUrl(new FetchRoute(queryParams, action));
    try {
      const response = await this.http.fetch(url);
      return this.handleResponse<TListItem[]>(response, reducer);
    } catch (ex) {
      return this.handleErrorResponse<TListItem[]>(ex as Response);
    }
  }

  private logResponse(response: Response) {
    log.error(`Status: ${response?.status}, StatusText: ${response?.statusText}`, response);
  }

  private async handleResponse<T>(response: Response, reducer?: Reducer<IState, T[]>): Promise<void | T> {
    this.serverMessageService.clearMessages();

    if (!response.ok) {
      this.handleErrorResponse(response);
      if (reducer) {
        return this.store.dispatch(reducer, undefined);
      }
    }

    let data: T;

    const serverResponse = (await response.json()) as IServerResponse<T>;

    if (serverResponse.isError) {
      await this.serverMessageService.setErrorMessage(serverResponse.message);
      data = undefined!;
    }

    if (!serverResponse.isError && serverResponse.message) {
      await this.serverMessageService.setMessage(serverResponse.message);
    }

    data = serverResponse.dto
      ? serverResponse.dto
      : serverResponse as unknown as T;

    if (reducer) {
      return this.store.dispatch(reducer, data as T);
    }
    return data as T;
  }

  private async handleErrorResponse<T>(response: Response) {
    this.logResponse(response);

    if (!response.status) {
      throw response;
    }

    switch (response.status) {
      case 400: {
        const text = (await response.text()) as string;
        try {
          const parsed = JSON.parse(text);
          this.store.dispatch(serverErrorMessageAction, parsed.message);
          log.error(parsed.message + "\n" + parsed.stackTrace);
        } catch (e) {
          log.error(text);
          throw e;
        }
        break;
      }
      case 401:
        this.store.dispatch(logoutAction);
        this.router.navigateToRoute("login", { httpStatusCode: response.status }, { trigger: true, replace: false });
        break;
      case 403:
        this.store.dispatch(logoutAction);
        this.router.navigateToRoute("login", { httpStatusCode: response.status }, { trigger: true, replace: false });
        break;
      default: {
        const serverError = (await response.text()) as string;
        this.store.dispatch(serverErrorMessageAction, serverError);
        log.error(serverError);
        this.router.navigateToRoute("login", { httpStatusCode: response.status }, { trigger: true, replace: false });
        throw serverError;
      }
    }
    return {} as T;
  }
}
