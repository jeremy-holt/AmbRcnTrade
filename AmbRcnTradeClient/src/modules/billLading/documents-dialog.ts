import { DeleteDialog } from "./../../dialogs/delete-dialog";
import { DialogController, DialogService } from "aurelia-dialog";
import { IBillLading } from "interfaces/shipping/IBillLading";
import { autoinject } from "aurelia-dependency-injection";

@autoinject
export class DocumentsDialog {
  protected model: IBillLading = undefined!;

  constructor(
    private controller: DialogController,
    private dialogService: DialogService
  ) { }

  protected activate(model: IBillLading) {
    this.model = model;
  }

  protected addDocument() {
    this.model.documents.push({ name: undefined, submitted: undefined, received: undefined, notes: undefined });
  }

  protected deleteDocument(index: number) {
    this.dialogService.open({
      viewModel: DeleteDialog,
      model: { header: "Delete document", body: "Are you sure you wish to delete this document?" }
    }).whenClosed(result => {
      if (!result.wasCancelled) {
        this.model.documents.splice(index, 1);
      }
    });
  }

  protected okClicked() {
    this.controller.ok();
  }
}
