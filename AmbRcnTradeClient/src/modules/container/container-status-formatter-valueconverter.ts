import { CONTAINER_STATUS_LIST } from "./../../constants/app-constants";
import { autoinject } from "aurelia-framework";
import { ContainerStatus } from "constants/app-constants";

@autoinject
export class ContainerStatusFormatterValueConverter {

  public toView(status: ContainerStatus) {
    return CONTAINER_STATUS_LIST.find(c => c.id === status)?.name;
  }
}
