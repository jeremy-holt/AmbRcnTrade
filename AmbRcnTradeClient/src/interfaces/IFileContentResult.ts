export interface IFileContentResult {
  fileContents: string;
  contentType: string;
  fileDownloadName: string;
  lastModified: string;
  entityTag?: string;
  enableRangeProcessing?: boolean;
}
