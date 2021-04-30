import { ImageType } from "constants/app-constants";

export interface IFileImage {
  objectUrl: string;
  name: string;
  size: number;
  imageType: ImageType;
  type: string;
  caption: string;
  file: File
  selected: boolean;
}
