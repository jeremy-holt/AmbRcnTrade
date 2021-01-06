import { DialogController } from "aurelia-dialog";
import { EventAggregator } from "aurelia-event-aggregator";
import { autoinject } from "aurelia-framework";
import { connectTo } from "aurelia-store";
import { BillLadingAttachmentsService } from "../../services/billLading-attachments-service";

@autoinject
@connectTo()
export class BillLadingUploadDialog {
  public formData: FormData = undefined!;
  public model: {billLadingId:string};

  constructor(
    private fileUploadService: BillLadingAttachmentsService,
    private ea: EventAggregator,
    private controller: DialogController
  ) { }

  protected activate(model: {billLadingId:string}) {
    this.model = model;
  }

  protected async upload(event: CustomEvent) {
    if (event?.detail) {
      this.ea.publish("uploadingImage", true);

      this.formData = event.detail;

      const headers = new Headers();
      headers.append("id", this.model.billLadingId);

      await this.fileUploadService.uploadImages(this.formData, headers);
      
      this.ea.publish("uploadingImage", false);

      this.controller.cancel();
    }
  }
}
