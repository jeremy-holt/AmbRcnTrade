import { QueryId } from "./../models/QueryId";
import { FetchPostRoute } from "./FetchPostRoute";

export class FetchRoute extends FetchPostRoute {
  constructor(public readonly params: QueryId[] | null, action?: string) {
    super(action);
  }
}
