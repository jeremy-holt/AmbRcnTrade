import { IContainer } from "../../interfaces/contract-interfaces/IContainer";
import moment from "moment";

export class ContainerFormatValueConverter {
  public toView(container: IContainer) {
    let text = "";

    if (container.vesselName) {
      text += `"${container.vesselName}"`;
    }

    if(container.blNumber){
      text += `, B/L ${container.blNumber} `;
    }

    if (container.blDate) {
      text += `dd ${moment(container.blDate).format("DD/MM/YYYY")}`;
    }

    if (container.containerNumber) {
      text += `, Cont No: ${container.containerNumber} `;
    }

    if(container.courierAwb){
      text +=`, AWB ${container.courierAwb}`;
    }

    text = text.length > 0 ? `Shipped per ${text}` : "";
    return text;
  }

}

