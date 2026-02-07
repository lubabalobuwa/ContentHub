import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environments';
import { BlobUploadRequest, BlobUploadResponse } from '../models/blob-upload.model';
import { firstValueFrom } from 'rxjs';

const MAX_FILE_SIZE = 5 * 1024 * 1024;
const ALLOWED_TYPES = ['image/jpeg', 'image/png'];

@Injectable({ providedIn: 'root' })
export class UploadService {
  private readonly baseUrl = `${environment.apiBaseUrl}/uploads`;

  constructor(private http: HttpClient) {}

  async uploadContentImage(contentId: string, file: File, rowVersion: string): Promise<void> {
    this.validateFile(file);
    const sas = await this.requestContentSas(contentId, file);
    await this.uploadToBlob(sas.uploadUrl, file);
    await this.completeContentUpload(contentId, sas.blobName, rowVersion);
  }

  async uploadProfileImage(file: File): Promise<void> {
    this.validateFile(file);
    const sas = await this.requestProfileSas(file);
    await this.uploadToBlob(sas.uploadUrl, file);
    await this.completeProfileUpload(sas.blobName);
  }

  private requestContentSas(contentId: string, file: File): Promise<BlobUploadResponse> {
    const payload: BlobUploadRequest = {
      fileName: file.name,
      contentType: file.type,
      contentLength: file.size
    };
    return firstValueFrom(
      this.http.post<BlobUploadResponse>(`${this.baseUrl}/content/${contentId}/sas`, payload)
    );
  }

  private requestProfileSas(file: File): Promise<BlobUploadResponse> {
    const payload: BlobUploadRequest = {
      fileName: file.name,
      contentType: file.type,
      contentLength: file.size
    };
    return firstValueFrom(
      this.http.post<BlobUploadResponse>(`${this.baseUrl}/profile/sas`, payload)
    );
  }

  private async uploadToBlob(uploadUrl: string, file: File): Promise<void> {
    const response = await fetch(uploadUrl, {
      method: 'PUT',
      headers: {
        'x-ms-blob-type': 'BlockBlob',
        'Content-Type': file.type
      },
      body: file
    });

    if (!response.ok) {
      throw new Error('Blob upload failed.');
    }
  }

  private completeContentUpload(contentId: string, blobName: string, rowVersion: string): Promise<void> {
    return firstValueFrom(
      this.http.post<void>(`${this.baseUrl}/content/${contentId}/complete`, { blobName, rowVersion })
    );
  }

  private completeProfileUpload(blobName: string): Promise<void> {
    return firstValueFrom(
      this.http.post<void>(`${this.baseUrl}/profile/complete`, { blobName })
    );
  }

  private validateFile(file: File): void {
    if (!ALLOWED_TYPES.includes(file.type)) {
      throw new Error('Only JPG and PNG images are allowed.');
    }
    if (file.size <= 0 || file.size > MAX_FILE_SIZE) {
      throw new Error('Image must be between 1 byte and 5 MB.');
    }
  }
}
