import { ImageType } from "./../../constants/app-constants";
export interface IAttachmentInfo {
    name: string;
    route: string;
    size: number;
    displayName: string;
    imageType: ImageType
}
