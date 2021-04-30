import { autoinject } from "aurelia-framework";
import { QueryId } from "./QueryId";
import { FetchRoute } from "./FetchRoute";
import { LanguageService } from "./language-service";

@autoinject
export class GetUrlService {
  constructor(
    private baseUrl: string
  ) {
  }

  public setBaseUrl(baseUrl: string) {
    this.baseUrl = baseUrl;
  }

  public get culture() {
    const culture = LanguageService.culture;
    return culture;
  }

  public getPostUrl(action: string) {
    return this.getUrl(new FetchRoute([], action));
  }

  public getUrl(route: FetchRoute): string {
    const url = this.baseUrl;
    let action = "";
    let seperator = "?";
    let params = "";

    if (!route.params && !route.action) {
      return url;
    }

    if (route.action) {
      action += "/" + route.action;
    }

    if (route.params) {
      const parseValue = (value: any) => value === undefined || value === null ? "" :  value.toString();
      const reducer = (current: string, next: QueryId) => current + (next.value ? `${next.name}=${parseValue(next.value)}&` : "");

      params = "?" + route.params.reduce(reducer, "");
      seperator = "";
    }

    const culture = `${seperator}culture=${this.culture}`;

    return url + action + params + culture;
  }

  public isValidRavenId(id: string) {
    const r = new RegExp(".[-||\\/]\\d*-[A,B,C,a,b,c]");
    return r.test(id);
  }

  public validateInputId(id: string) {
    const arr = id.split("/");
    if (arr.length > 2) {
      return false;
    }
    if (arr.length === 2) {
      const lastTest = arr[0].split("-");
      if (lastTest.length === 3) {
        return false;
      }
      return this.isValidRavenId(id);
    }
    return arr.every(c => this.isValidRavenId(c));
  }
}
