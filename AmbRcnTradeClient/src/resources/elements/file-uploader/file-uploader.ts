import { DialogController } from "aurelia-dialog";
import { autoinject, bindable, observable } from "aurelia-framework";
import * as blobUtil from "blob-util";
import { IFileImage } from "interfaces/IFileImage";
import { ImageType } from "./../../../constants/app-constants";

@autoinject
export class FileUploader {
  @observable public selectedFiles: FileList = undefined!;
  @bindable public fileNameMapper: (fileName: string, prefix: string) => void = undefined!;

  public previewImages: IFileImage[] = [];
  protected hasUploaded = false;
  private selectAllFiles = true;

  constructor(
    private el: Element,
    protected controller: DialogController
  ) { }

  protected async selectedFilesChanged(files: FileList) {
    this.previewImages = [];

    for (let i = 0; i < files.length; i++) {
      const file = files[i];

      const { objectData, imageType } = await this.getObjectInfoFromFile(file);

      this.previewImages.push({
        objectUrl: objectData,
        name: file.name,
        size: file.size,
        type: file.type,
        imageType,
        caption: null,
        file,
        selected: true
      });

      this.hasUploaded = false;
    }
  }

  private async getObjectInfoFromFile(file: File): Promise<{ objectData: string; imageType: ImageType; }> {
    if (file.type.startsWith("image/")) {
      return { objectData: blobUtil.createObjectURL(file), imageType: ImageType.image };

    } else if (file.type === "application/pdf") {
      const objectData = await blobUtil.blobToDataURL(file);
      return { objectData, imageType: ImageType.pdf };

    } else {
      return { objectData: "", imageType: ImageType.other };
    }
  }

  protected get canUpload() {
    return this.previewImages.length > 0;
  }

  public upload() {
    const formData = new FormData();
    this.previewImages.forEach(c => {
      if (c.selected) {
        const fileName = c.caption ? `${c.caption}.${this.getFileType(c.file.name)}` : c.name;
        formData.append(fileName, c.file);
      }
    });

    this.notify(formData);
    this.hasUploaded = true;
  }

  protected selectDeselect() {
    this.selectAllFiles = !this.selectAllFiles;
    this.previewImages.forEach(c => c.selected = this.selectAllFiles);
  }

  private getFileType(fileName: string) {
    return fileName.split(".")[1];
  }

  protected notify(formData: FormData) {
    this.el.dispatchEvent(new CustomEvent("upload", {
      bubbles: true,
      detail: formData
    }));
  }
}


