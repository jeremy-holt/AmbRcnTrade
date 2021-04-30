import { BillLadingAttachmentsService } from "./../../services/billLading-attachments-service";
import { DialogController, DialogService } from "aurelia-dialog";
import { autoinject } from "aurelia-framework";
import { Router } from "aurelia-router";
import { connectTo } from "aurelia-store";
import { IAttachmentInfo } from "core/interfaces/IAttachmentInfo";
import { IState } from "store/state";
import { IDeleteAttachmentRequest } from "core/interfaces/IDeleteAttachmentRequest";
import { DeleteDialog } from "dialogs/delete-dialog";


@autoinject
@connectTo()
export class DocumentsDownloadDialog {
  protected state: IState = undefined!;
  public model: { billLadingId: string };
  public attachmentRoutes: IAttachmentInfo[] = [];

  constructor(
    protected controller: DialogController,
    private billLadingAttachmentsService: BillLadingAttachmentsService,
    private dialogService: DialogService,
    private router: Router
  ) { }

  protected async activate(model: { billLadingId: string }) {
    this.model = model;
    await this.billLadingAttachmentsService.getDocumentRoutes(model.billLadingId);
  }

  protected closeViewer() {
    this.controller.cancel();
  }

  protected async deleteDocument(fileName: string) {
    this.dialogService.open({
      viewModel: DeleteDialog
    }).whenClosed(async result => {
      if (!result.wasCancelled) {

        const requests: IDeleteAttachmentRequest[] = [
          { contractId: this.model.billLadingId, fileName }
        ];

        await this.billLadingAttachmentsService.deleteAttachments(requests);
        await this.billLadingAttachmentsService.getDocumentRoutes(this.model.billLadingId);
      }
    });

  }
}
