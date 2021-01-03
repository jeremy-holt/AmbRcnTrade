import { DialogController } from "aurelia-dialog";
import { ContainerService } from "./../../services/container-service";
import { autoinject } from "aurelia-framework";

@autoinject
export class UnstuffContainerDialog {
  constructor(
    private containerService: ContainerService,
    private controoler: DialogController
  ) { }  
}
