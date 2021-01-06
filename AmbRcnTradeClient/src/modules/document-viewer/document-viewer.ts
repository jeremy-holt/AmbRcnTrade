import { autoinject } from "aurelia-framework";
import { connectTo } from "aurelia-store";
import { IState } from "store/state";
import { ImageType } from "./../../constants/app-constants";

@autoinject
@connectTo()
export class ContractDocumentViewer {
  protected state: IState = undefined!;
  protected route: string;
  protected displayName: string;
  protected imageType: ImageType;
  private paramsIndex: number = undefined!;

  protected activate(params: { id: number }) {    
    this.paramsIndex = params.id;
  }

  protected bind() {    
    const attachmentRoutes = this.state.attachmentRoutes[this.paramsIndex];
    this.route = attachmentRoutes.route;
    this.imageType = attachmentRoutes.imageType;
    this.displayName = attachmentRoutes.displayName;
  }
}
