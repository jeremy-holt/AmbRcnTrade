import { HttpClient } from "aurelia-fetch-client";
import { autoinject } from "aurelia-framework";

@autoinject
export class Progressbar {
  constructor(public http: HttpClient) { }
}
