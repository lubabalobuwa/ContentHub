import { Component, ChangeDetectionStrategy, ChangeDetectorRef, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ContentService } from '../../../../core/services/content.service';
import { UploadService } from '../../../../core/services/upload.service';
import { Content } from '../../../../core/models/content.model';
import { marked } from 'marked';
import Cropper from 'cropperjs';

@Component({
  selector: 'app-create-content-page',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './create-content.page.html',
  styleUrl: './create-content.page.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CreateContentPage {
  @ViewChild('cropperImage') cropperImageRef?: ElementRef<HTMLImageElement>;

  title = '';
  body = '';
  selectedImage: File | null = null;
  croppedPreviewUrl: string | null = null;
  isCropping = false;
  private cropper?: Cropper;
  private cropSourceUrl: string | null = null;

  isSubmitting = false;
  isUploading = false;
  error: string | null = null;

  constructor(
    private contentService: ContentService,
    private uploadService: UploadService,
    private router: Router,
    private cdr: ChangeDetectorRef
  ) {
  }

  get markdownPreview(): string {
    return marked.parse(this.body || '', { breaks: true, gfm: true }) as string;
  }

  insertSyntax(before: string, after = '') {
    const selectionStart = this.body.length;
    const insertion = `${before}${after}`;
    this.body = `${this.body.slice(0, selectionStart)}${insertion}${this.body.slice(selectionStart)}`;
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) {
      this.selectedImage = null;
      this.croppedPreviewUrl = null;
      return;
    }

    this.startCrop(file);
  }

  submit() {
    this.error = null;
    this.body = this.body.trim();

    if (!this.title.trim()) {
      this.error = 'Title is required.';
      return;
    }
    if (this.title.length > 200) {
      this.error = 'Title must be under 200 characters.';
      return;
    }
    if (!this.body.trim()) {
      this.error = 'Body is required.';
      return;
    }
    this.isSubmitting = true;

    this.contentService.create({
      title: this.title.trim(),
      body: this.body.trim()
    }).subscribe({
      next: async (response) => {
        if (this.selectedImage) {
          try {
            this.isUploading = true;
            const content = await this.fetchContent(response.id);
            if (!content?.rowVersion) {
              throw new Error('Missing row version for image upload.');
            }
            await this.uploadService.uploadContentImage(response.id, this.selectedImage, content.rowVersion);
          } catch {
            this.error = 'Content created, but image upload failed.';
          } finally {
            this.isUploading = false;
          }
        }
        this.isSubmitting = false;
        this.router.navigateByUrl('/drafts');
      },
      error: () => {
        this.isSubmitting = false;
        this.error = 'Failed to create content.';
      }
    });
  }

  private fetchContent(id: string): Promise<Content | null> {
    return new Promise(resolve => {
      this.contentService.getById(id).subscribe({
        next: content => resolve(content),
        error: () => resolve(null)
      });
    });
  }

  private startCrop(file: File) {
    this.cleanupCropper();
    this.cropSourceUrl = URL.createObjectURL(file);
    this.isCropping = true;
    this.cdr.markForCheck();

    setTimeout(() => {
      const image = this.cropperImageRef?.nativeElement;
      if (!image || !this.cropSourceUrl) return;
      image.src = this.cropSourceUrl;
      this.cropper = new Cropper(image, {
        aspectRatio: 16 / 9,
        viewMode: 1,
        autoCropArea: 1,
        responsive: true
      });
    }, 0);
  }

  cancelCrop() {
    this.cleanupCropper();
    this.selectedImage = null;
    this.croppedPreviewUrl = null;
    this.isCropping = false;
    this.cdr.markForCheck();
  }

  applyCrop() {
    if (!this.cropper) return;
    const canvas = this.cropper.getCroppedCanvas();
    canvas.toBlob(blob => {
      if (!blob) return;
      const file = new File([blob], `cover-${Date.now()}.jpg`, { type: 'image/jpeg' });
      this.selectedImage = file;
      this.croppedPreviewUrl = URL.createObjectURL(blob);
      this.isCropping = false;
      this.cleanupCropper();
      this.cdr.markForCheck();
    }, 'image/jpeg', 0.92);
  }

  private cleanupCropper() {
    this.cropper?.destroy();
    this.cropper = undefined;
    if (this.cropSourceUrl) {
      URL.revokeObjectURL(this.cropSourceUrl);
      this.cropSourceUrl = null;
    }
  }
}
