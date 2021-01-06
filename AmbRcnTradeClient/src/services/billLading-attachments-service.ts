import { HttpClient } from "aurelia-fetch-client";
import { autoinject } from "aurelia-framework";
import { Router } from "aurelia-router";
import { Store } from "aurelia-store";
import _ from "lodash";
import { QueryId } from "models/QueryId";
import { IState } from "store/state";
import { IAttachmentInfo } from "../interfaces/IAttachmentInfo";
import { IDeleteAttachmentRequest } from "../interfaces/IDeleteAttachmentRequest";
import { FetchService } from "./fetch-service";
import { noOpAction } from "./no-op-action";


@autoinject
export class BillLadingAttachmentsService extends FetchService {
  constructor(
    http: HttpClient,
    store: Store<IState>,
    router: Router
  ) {
    super("api/billLadingAttachments", http, store, router);
    store.registerAction("attachmentRoutesAction", attachmentRoutesAction);
  }

  public async uploadImages(formData: FormData, headers: Headers) {
    return await super.postImage(formData, headers, "uploadDocuments");
  }

  public async getDocumentRoutes(contractId: string) {
    return await super.getMany([new QueryId("contractId", contractId)], "getDocumentRoutes", attachmentRoutesAction);
  }

  public async getAttachment(contractId: string, fileName: string) {
    await super.get([new QueryId("contractId", contractId), new QueryId("fileName", fileName)], "getAttachment", noOpAction);
  }

  public async deleteAttachments(requests: IDeleteAttachmentRequest[]) {
    return await super.post(requests, "deleteAttachments");
  }
}

export function attachmentRoutesAction(state: IState, response: IAttachmentInfo[]) {
  const newState = _.cloneDeep(state);
  newState.attachmentRoutes = response;
  return newState;
}
