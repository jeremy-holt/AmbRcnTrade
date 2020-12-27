import { autoinject, bindable } from "aurelia-framework";
import { IFileContentResult } from "../../../interfaces/IFileContentResult";

@autoinject
export class ImageViewer {
  @bindable public model: IFileContentResult = undefined!;
}
