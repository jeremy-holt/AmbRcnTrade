import { LogManager } from "aurelia-framework";
export const logId = (obj:Record<string,unknown>) => `${obj.constructor.name}.`;
export const log = LogManager.getLogger("ExportManager");

