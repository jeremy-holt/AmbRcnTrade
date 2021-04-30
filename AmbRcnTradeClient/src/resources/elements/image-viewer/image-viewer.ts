import { autoinject, bindable } from "aurelia-framework";
import { IFileContentResult } from "core/interfaces/IFileContentResult";

@autoinject
export class ImageViewer {
  @bindable public model: IFileContentResult = undefined!;
}
