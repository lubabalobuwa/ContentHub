export interface BlobUploadRequest {
  fileName: string;
  contentType: string;
  contentLength: number;
}

export interface BlobUploadResponse {
  uploadUrl: string;
  blobUrl: string;
  blobName: string;
}
