import { CONTAINER_STATUS_LIST } from "./../../constants/app-constants";
import { autoinject } from "aurelia-framework";
import { ContainerStatus } from "constants/app-constants";

@autoinject
export class ContainerStatusFormatterValueConverter {

  public toView(status: ContainerStatus | string) {
 

    return CONTAINER_STATUS_LIST.find(c => c.id === status || c.id.toString()===status.toString())?.name;
  }
}
