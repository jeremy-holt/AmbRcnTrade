import { LogManager } from "aurelia-framework";
export const logId = obj => `${obj.constructor.name}.`;
export const log = LogManager.getLogger("ExportManager");

